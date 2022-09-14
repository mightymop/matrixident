using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Database;
using MatrixIdent.Models;
using MatrixIdent.Services;
using System.Text.Json.Nodes;

namespace MatrixIdent.Controllers
{
    [Route("/_matrix/identity/v2")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly ConfigService _configService;

        public InvitationController(DBService dbService, ConfigService configService)
        {
            _dbService = dbService;
            _configService = configService; 
        }


        [HttpPost("/_matrix/identity/v2/sign-ed25519")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> sign(SignItem itm)
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

            InvitationRequestItem[] item = await _dbService.getInvitationItemByToken(itm.token);
            if (item!=null)
            {
                dynamic result = new
                {
                    mxid = itm.mxid,
                    sender = item[0].sender,
                    token = itm.token
                };

                string priv;
                string pub;
                var signed = Functions.signJson(result,out priv, out pub);
                Key k = new Key();
                k.public_key = pub;
                k.private_key = priv;
                k.expiration_timestamp = Functions.timestamp() + _configService.getEphemeralKeyExpirationHours() * 60 * 60;
                k.identifier = "ed25519:"+Guid.NewGuid().ToString();

                _dbService.addKey(k);
                result["signatures"] = JsonNode.Parse("{\""+_configService.getMatrixDomain()+"\":{\""+k.identifier+"\":\""+signed+"\"}}");

                return Ok(result);
            }
            else
            {
                return NotFound(ErrorType.M_NOT_FOUND.ToString());
            }            
        }


        [HttpPost("/_matrix/identity/v2/store-invite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> store_invite(InvitationRequestItem itm)
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

            if (itm.medium!="email")
            {
                return BadRequest(new ErrorType("M_UNRECOGNIZED", "For invitations the medium is not supported by the server!").ToJSON());
            }

            itm.token = Functions.generateToken(20);

            Key k = Functions.createED225519Key(_configService.getEphemeralKeyExpirationHours());
            itm.key = k.identifier;

            if (await _dbService.addOrUpdateInvitationItem(itm))
            {
                if (await _dbService.addKey(k))
                {
                    string display_name = itm.address;
                    Functions.sendInvitationMail(itm.address, display_name, itm.room_name, itm.room_avatar_url, itm.room_type, _configService);

                    string res = "{" +
                                    "\"application/json\": {" +
                                        "\"display_name\": \"" + display_name + "\"," +
                                        "\"public_keys\": [" +
                                            "\"" + _configService.getFirstServerPublicKey() + "\"," +
                                            "\"" +k.public_key+ "\"" +
                                        "]," +
                                    "\"token\": \"" + itm.token + "\"" +
                                    "}," +
                                    "\"public_keys\": [null]" +
                                  "}";
                    return Ok(res);
                }
            }

            return Problem(new ErrorType("DATABASE ERROR", "Could not update / insert data (in)to database.").ToString());
        }
    }
}

