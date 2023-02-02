using MatrixIdent.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using MatrixIdent.Models;
using System.Text;

using log4net;


namespace MatrixIdent.Controllers
{
    [ApiController]
    public class KeyController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly ConfigService _configService;

        private ILog log = LogManager.GetLogger(typeof(KeyController));

        public KeyController(DBService dbService, ConfigService configService)
        {
            this._configService = configService;
            this._dbService = dbService;
        }
     

        [HttpGet("/_matrix/identity/v2/pubkey/ephemeral/isvalid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ephemeralisvalid(string public_key)
        {
            var key = await _dbService.getKeyFromPublicKey(public_key);
            if (key==null||key.expiration_timestamp<Functions.timestamp())
            {
                return Ok(new { valid = false });
            }
            else
            {
                return Ok(new { valid = true });
            }
        }


        [HttpGet("/_matrix/identity/v2/pubkey/isvalid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> isvalid(string public_key)
        {
            /*var base64EncodedBytes = System.Convert.FromBase64String(public_key);
            string decodedString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);*/
            bool isvalid = _configService.checkPublicKey(public_key);
            return Ok(new { valid = isvalid });
        }

        [HttpGet("/_matrix/identity/v2/pubkey/{keyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> getKey(string keyId) //algorithm:identifier 
        {
            string public_key = _configService.getServerPublicKey(keyId);
            if (public_key != null)
            {
                /*var plainTextBytes = Encoding.UTF8.GetBytes(public_key);
                string b64 = System.Convert.ToBase64String(plainTextBytes);*/
                return Ok(new { public_key = public_key });
            }
            else
            {
                return NotFound(ErrorType.M_NOT_FOUND.ToJSON());
            }
        }
    }
}

