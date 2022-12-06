using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Models;
using MatrixIdent.Services;
using System.Net;
using System.Text.Json.Nodes;
using log4net;

namespace MatrixIdent.Controllers
{
    [Route("/_matrix/identity/v2/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly ConfigService __configService;
        private ILog log = LogManager.GetLogger(typeof(AccountController));

        public AccountController(DBService dbService, ConfigService configService)
        {
            _dbService = dbService;
            __configService = configService;    
        }

        [HttpPost("/_matrix/identity/v2/account/register")]
        [ProducesResponseType(typeof(AuthItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> register(AuthItem itm)
        {
            /*
             //REQUEST BODY:

            {
              "access_token": "string",
              "token_type": "string",
              "matrix_server_name": "string",
              "expires_in": 0
            }

            //RESPONSE: 

            {
              "token": "abc123_OpaqueString"
            }
             */

            if (itm == null ||
                (itm.matrix_server_name == null || itm.matrix_server_name == "" ||
                 itm.access_token == null || itm.access_token == "" ||
                 itm.token_type == null || itm.token_type == ""))
            {
                return BadRequest(ErrorType.M_MISSING_PARAMS.ToString());
            }

            if (itm != null && itm.token != null)
            {
                return BadRequest(ErrorType.M_INVALID_PARAM.ToString());
            }

            itm.user_id = getUserFromHomeserver(itm.access_token);

            if (itm.user_id != null)
            {                
                string strtoken = await _dbService.saveToken(itm);
                Response.Headers.Add("Authorization", "Bearer " + strtoken);
                return Ok(new { token = strtoken });
            }
            else
            {
                return Problem(ErrorType.M_UNKNOWN.ToString());
            }
        }

        private string getUserFromHomeserver(string access_token)
        {   
            string url = "https://" + __configService.getHomeserverHost() + ":" + Convert.ToString(__configService.getHomeserverPort()) + "/_matrix/federation/v1/openid/userinfo?access_token="+access_token;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "GET";

           // httpRequest.ContentType = "application/json";

          /*  using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }*/

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    JsonNode json = JsonNode.Parse(result);
                    return json["sub"].ToString();
                }
            }

            return null;
           
        }

        [HttpPost("/_matrix/identity/v2/account/logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> logout()
        {

            string? auth = Functions.getToken(Request);
            if (auth == null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            if (await _dbService.deleteToken(auth))
            {
                return Ok(new {});
            }
            else
            {
                return Unauthorized(new ErrorType("M_UNKNOWN_TOKEN", "Unrecognised access token.").ToJSON());
            }
        }


        [HttpGet("/_matrix/identity/v2/account")]
        [ProducesResponseType(typeof(AuthItem), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> account()
        {
         string token = Functions.getToken(Request);
            if (token==null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED","YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            string access_token = await _dbService.getAccessToken(token);
            if (access_token == null)
            {
                return NotFound(ErrorType.M_NOT_FOUND.ToJSON());
            }

            AuthItem itm = await _dbService.getAuthItem(token);
            
            return Ok(new { user_id = itm.user_id});
        }
    }
}

