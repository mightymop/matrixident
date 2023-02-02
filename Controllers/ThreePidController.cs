using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Models;
using MatrixIdent.Services;
using System.Text.Json.Nodes;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Net;
using log4net;


namespace MatrixIdent.Controllers
{
  
    [ApiController]
    public class ThreePidController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly ConfigService _configService;

        private ILog log = LogManager.GetLogger(typeof(ThreePidController));

        public ThreePidController(DBService dbService, ConfigService configService)
        {
            _dbService = dbService;
            _configService = configService;
        }

        private void callOnBind(string address, string mxid, InvitationRequestItem[] invitations)
        {
            List<object> invites = new List<object>();
            for (int n=0;n<invitations.Length;n++)
            {
                var sig = JsonNode.Parse("{}");

                string privkey;
                string pubkey;
                string token = Functions.generateToken(20);
                string signature = Functions.signJson(new { mxid = mxid, token = token }, out privkey, out pubkey);
                sig[_configService.getMatrixDomain()] = JsonNode.Parse("{\"ed25519:0\": \""+signature+"\"}");

                var iv = new {
                    address = invitations[n].address,
                    medium = invitations[n].medium,
                    mxid = mxid,
                    room_id = invitations[n].room_id,
                    sender = invitations[n].sender,
                    signed = new
                    {
                        mxid = mxid,
                        signature = sig,
                        token = Functions.generateToken(10)
                    }
                }; 
                
                invites.Add(iv);
            }

            var json = new
            {
                address=address,
                invites = invites.ToArray(),
                medium = "email",
                mxid = mxid
            };

            string url = "https://" + _configService.getHomeserverHost() + ":" + Convert.ToString(_configService.getHomeserverPort()) + "/_matrix/federation/v1/3pid/onbind";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
            Console.WriteLine(httpResponse.StatusCode.ToString());
        }

        [HttpPost("/_matrix/identity/v2/3pid/bind")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> bind(ThreePidRequestItem itm)
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            if (!await _dbService.checkToken(auth))
            {
                return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToJSON());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            var email_val = await _dbService.getEmailValidationItemBySecretAndSid(itm.client_secret, itm.sid);
            var msisdn_val = await _dbService.getMsisdnValidationItemBySecretAndSid(itm.client_secret, itm.sid);

            ThreePidResponseItem resultItem = new ThreePidResponseItem();
            if (itm.mxid != null)
            {
                resultItem.mxid = itm.mxid;
            }
            else
            {
                var authItem = await _dbService.getAuthItem(auth);
                resultItem.mxid = authItem.user_id;
            }

            long timestamp = Functions.timestamp();
            resultItem.ts = timestamp;
            resultItem.not_before = timestamp;
            resultItem.not_after = timestamp + _configService.get3PidExpireAfterHours() * 60 * 60;

            if (email_val!=null)
            {
                if (email_val.expire_after<=Functions.timestamp())
                {
                    await _dbService.deleteEmailValidationItem(email_val.email);
                    return BadRequest(new ErrorType("M_SESSION_EXPIRED", "The session has timed out.").ToJSON());
                }

                if ((bool)!email_val.success)
                {                  
                    return BadRequest(new ErrorType("M_SESSION_NOT_VALIDATED", "The session has not been validated.").ToJSON());
                }
               
                resultItem.address = email_val.email;
                resultItem.medium = "email";
             
                if (await _dbService.addOrUpdate3PIDItem(resultItem))
                {
                    //check for invitations

                    var invitationItems = await _dbService.getInvitationItem(resultItem.address);
                    if (invitationItems!=null)
                    {
                        callOnBind(resultItem.address, resultItem.mxid, invitationItems);

                        for (int n=0;n< invitationItems.Length;n++)
                        {
                            //call /3pid/onbind
                            
                            await _dbService.deleteInvitation(invitationItems[n].address, invitationItems[n].room_id, invitationItems[n].medium, invitationItems[n].sender);
                        }
                        
                    }

                    dynamic result = new
                    {
                        address = resultItem.address,
                        medium = "email",
                        mxid = resultItem.mxid,
                        not_after = resultItem.not_after,
                        not_before = resultItem.not_before,
                        ts= resultItem.ts
                    };

                    string priv;
                    string pub;
                    var signed = Functions.signJson(result, out priv, out pub);
                    Key k = new Key();
                    k.public_key = pub;
                    k.private_key = priv;
                    k.expiration_timestamp = Functions.timestamp() + _configService.getEphemeralKeyExpirationHours() * 60 * 60;
                    k.identifier = "ed25519:" + Guid.NewGuid().ToString();

                    _dbService.addKey(k);
                    result["signatures"] = JsonNode.Parse("{\"" + _configService.getMatrixDomain() + "\":{\"" + k.identifier + "\":\"" + signed + "\"}}");

                    return Ok(result);
                }
                else
                {
                    return Problem(new ErrorType("DATABASE ERROR", "Could not update / insert data (in)to database.").ToString());
                }
            }
            else
            if (msisdn_val != null)
            {
                if (msisdn_val.expire_after <= Functions.timestamp())
                {
                    await _dbService.deleteMsisdnValidationItem(msisdn_val.phone_number);
                    return BadRequest(new ErrorType("M_SESSION_EXPIRED", "The session has timed out.").ToJSON());
                }

                if ((bool)!msisdn_val.success)
                {
                    return BadRequest(new ErrorType("M_SESSION_NOT_VALIDATED", "The session has not been validated.").ToJSON());
                }

                resultItem.address = msisdn_val.phone_number;
                resultItem.medium = "msisdn";

                if (await _dbService.addOrUpdate3PIDItem(resultItem))
                {
                    dynamic result = new
                    {
                        address = resultItem.address,
                        medium = "msisdn",
                        mxid = resultItem.mxid,
                        not_after = resultItem.not_after,
                        not_before = resultItem.not_before,
                        ts = resultItem.ts
                    };

                    string priv;
                    string pub;
                    var signed = Functions.signJson(result, out priv, out pub);
                    Key k = new Key();
                    k.public_key = pub;
                    k.private_key = priv;
                    k.expiration_timestamp = Functions.timestamp() + _configService.getEphemeralKeyExpirationHours() * 60 * 60;
                    k.identifier = "ed25519:" + Guid.NewGuid().ToString();

                    _dbService.addKey(k);
                    result["signatures"] = JsonNode.Parse("{\"" + _configService.getMatrixDomain() + "\":{\"" + k.identifier + "\":\"" + signed + "\"}}");

                    return Ok(result);
                }
                else
                {
                    return Problem(new ErrorType("DATABASE ERROR", "Could not update / insert data (in)to database.").ToString());
                }
            }

            return NotFound(ErrorType.M_NOT_FOUND.ToJSON());

        }


        [HttpPost("/_matrix/identity/v2/3pid/getValidated3pid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> getValidated3pid (BaseValidationItem itm)
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            if (!await _dbService.checkToken(auth))
            {
                return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToJSON());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            var email_val = await _dbService.getEmailValidationItemBySecretAndSid(itm.client_secret, itm.sid);
            var msisdn_val = await _dbService.getMsisdnValidationItemBySecretAndSid(itm.client_secret, itm.sid);
            if (email_val != null)
            {
                if ((bool)email_val.success)
                {
                    return Ok(new { address = email_val.email, medium = "email", validated_at = email_val.expire_after - _configService.getValidationExpireAfterMinutes()*60});
                }
                else
                {
                    return BadRequest(ErrorType.M_SESSION_NOT_VALIDATED.ToJSON());
                }
            }
            if (msisdn_val != null)
            {

                if ((bool)msisdn_val.success)
                {
                    return Ok(new { address = msisdn_val.phone_number, medium = "msisdn", validated_at = msisdn_val.expire_after - _configService.getValidationExpireAfterMinutes() * 60 });
                }
                else
                {
                    return BadRequest(ErrorType.M_SESSION_NOT_VALIDATED.ToJSON());
                }
            }

            return NotFound(ErrorType.M_NOT_FOUND.ToJSON());

        }

        [HttpPost("/_matrix/identity/v2/3pid/unbind")]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> unbind(BaseValidationItem itm)
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            if (!await _dbService.checkToken(auth))
            {
                return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToJSON());
            }
            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            var email_val = await _dbService.getEmailValidationItemBySecretAndSid(itm.client_secret, itm.sid);
            var msisdn_val = await _dbService.getMsisdnValidationItemBySecretAndSid(itm.client_secret, itm.sid);
            if (email_val != null)
            {
                if (await _dbService.delete3PIDItem(email_val.email,itm.mxid,"email"))
                {
                    await _dbService.deleteEmailValidationItem(email_val.email);
                    return Ok("{}");
                }
                else
                {
                    return Problem(new ErrorType("DATABASE ERROR", "Could not delete data from database.").ToString());
                }
            }
            if (msisdn_val != null)
            {
                if (await _dbService.delete3PIDItem(msisdn_val.phone_number, itm.mxid, "msisdn"))
                {
                    await _dbService.deleteMsisdnValidationItem(msisdn_val.phone_number);
                    return Ok("{}");
                }
                else
                {
                    return Problem(new ErrorType("DATABASE ERROR", "Could not delete data from database.").ToString());
                }
            }

            return NotFound(ErrorType.M_NOT_FOUND.ToJSON());
        }
    }
}

