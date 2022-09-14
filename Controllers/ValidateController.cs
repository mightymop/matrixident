using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Database;
using MatrixIdent.Models;
using MatrixIdent.Services;
using Org.BouncyCastle.Ocsp;
using System.Security.Cryptography;

namespace MatrixIdent.Controllers
{
    [Route("/_matrix/identity/v2/validate")]
    [ApiController]
    public class ValidateController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly ConfigService _configService;

        public ValidateController(DBService dbService, ConfigService configService)
        {
            _dbService = dbService;
            _configService = configService; 
        }

        [HttpPost("/_matrix/identity/v2/validate/email/requestToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> email_requestToken(EmailValidationRequestItem itm)
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
              //  return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToString());
            }

            if (!await _dbService.checkToken(auth))
            {
               // return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToString());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }


            if (!Functions.isValidMail(itm.email))
            {
                return BadRequest(new ErrorType("M_INVALID_EMAIL", "The email address provided was invalid.").ToJSON());
            }

            string sid = Functions.generateSessionID();
            string token = Functions.generateToken(10);

            string url = Request.Scheme + "://" + Request.Host + "/_matrix/identity/v2/validate/email/submitToken?token=" + token + "&sid=" + sid + "&client_secret=" + itm.client_secret;

            if (Functions.sendEmailValidationMail(itm.email, url, _configService))
            {
                itm.sid = sid;
                itm.token = token;

                itm.expire_after = Functions.timestamp() + 60 * _configService.getValidationExpireAfterMinutes();
                if (!await _dbService.addOrUpdateEmailValidationItem(itm))
                {
                    return Problem(new ErrorType("M_UNKNOWN", "Could not write to databae.").ToString());
                }
                return Ok(new { sid = sid });
            }
            else
            {
                return BadRequest(new ErrorType("M_EMAIL_SEND_ERROR", "The validation email could not be sent.").ToJSON()); 
            }

        }


        [HttpGet("/_matrix/identity/v2/validate/email/submitToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> email_submitToken()
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
                //   return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToString());
            }

            if (!await _dbService.checkToken(auth))
            {
                //   return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToString());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            Microsoft.Extensions.Primitives.StringValues data;
            Request.Query.TryGetValue("token", out data);
            string token = data;
            Request.Query.TryGetValue("client_secret", out data);
            string client_secret = data;
            Request.Query.TryGetValue("sid", out data);
            string sid = data;

            var result = await _dbService.getEmailValidationItemByTokenSidSecret(token, client_secret, sid);
            if (result == null)
            {
                return BadRequest(new ErrorType("M_UNKNOWN", "Validation failed").ToJSON());
            }

            if (!await _dbService.setTrueEmailValidationItem(token, client_secret, sid, true))
            {
                return Problem(new ErrorType("M_UNKNOWN", "Could not write to databae.").ToString());
            }
           
            return Ok("Email address is validated.");
        }

        [HttpPost("/_matrix/identity/v2/validate/email/submitToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> email_submitToken(BaseValidationItem itm)
        {
            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
             //   return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToString());
            }

            if (!await _dbService.checkToken(auth))
            {
             //   return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToString());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            var result = await _dbService.getEmailValidationItemByTokenSidSecret(itm.token, itm.client_secret, itm.sid);
            if (result==null)
            {
               return BadRequest(new ErrorType("M_UNKNOWN", "Validation failed").ToJSON());
            }

            if (!await _dbService.setTrueEmailValidationItem(itm.token, itm.client_secret, itm.sid, true))
            {
                return Problem(new ErrorType("M_UNKNOWN", "Could not write to databae.").ToString());
            }
            
            return Ok(new { success = true } );
        }

        [HttpPost("/_matrix/identity/v2/validate/msisdn/requestToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> msisdn_requestToken(MsisdnValidationRequestItem itm)
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


            if (!Functions.isValidPhonenumber(itm.phone_number))
            {
                return BadRequest(new ErrorType("M_INVALID_ADDRESS", "The email address provided was invalid.").ToJSON());
            }

            string sid = Functions.generateSessionID();
            string token = Functions.generateShortToken();

            if (Functions.sendPhondeValidationSMS(itm.phone_number, token, _configService))
            {
                itm.sid = sid;
                itm.token = token;
                if (!await _dbService.addOrUpdateMsisdnValidationItem(itm))
                {
                    return Problem(new ErrorType("M_UNKNOWN", "Could not write to databae.").ToString());
                }
                return Ok(new { sid = sid });
            }
            else
            {
                return BadRequest(new ErrorType("M_SMS_SEND_ERROR", "The validation sms could not be sent.").ToJSON());
            }

        }

        [HttpGet("/_matrix/identity/v2/validate/msisdn/submitToken")]
        [HttpPost("/_matrix/identity/v2/validate/msisdn/submitToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> msisdn_submitToken(BaseValidationItem itm)
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


            bool post = Request.Method == "POST";

            var result = await _dbService.getMsisdnValidationItemByTokenSidSecret(itm.token, itm.client_secret, itm.sid);
            if (result == null)
            {
                return BadRequest(new ErrorType("M_UNKNOWN", "Validation failed").ToJSON());
            }

            if (!await _dbService.setTruePhonenumberValidationItem(itm.token, itm.client_secret, itm.sid, true))
            {
                return Problem(new ErrorType("M_UNKNOWN", "Could not write to databae.").ToString());
            }

            if (post)
            {
                return Ok(new { success = true });
            }
            else
            {
                return Ok("Phone number was validated.");
            }

        }
    }
}

