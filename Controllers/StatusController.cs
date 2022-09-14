using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Services;
using System.Text;
using System.Net;
using Org.BouncyCastle.Utilities.Net;
using System.Text.Json.Nodes;

namespace MatrixIdent.Controllers
{
    [Route("/_matrix/identity/v2")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ConfigService _config;

        public StatusController(ConfigService config)
        {
            _config = config;
        }

        [HttpGet("/_matrix/identity/v2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult v2()
        {
            return Ok(new {});
        }

        [HttpGet("/_matrix/identity/api/v1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult v1()
        {
            return Ok(new {});
        }

        [HttpGet("/_matrix/identity/versions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult versions()
        {
            return Ok(new { versions = new[] { _config.getSpecVersion(), _config.getApiVersion() } });
        }

        [HttpGet("/_matrix/identity/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult test()
        {
          
            var data = "{" +
                    "\"address\": \"admin@mopsdom.de\"," +
                    "\"invites\": [" +
                        "{" +
                          "\"address\": \"admin@mopsdom.de\"," +
                          "\"medium\": \"email\"," +
                          "\"mxid\": \"@admin:mopsdom.de\"," +
                          "\"room_id\": \"!somewhere:mopsdom.de\"," +
                          "\"sender\": \"@mops:mopsdom.de\"," +
                          "\"signed\": { " +
                                "\"mxid\": \"@admin:mopsdom.de\"," +
                                "\"signatures\": { " +
                                            "\"mopsdom.de\": { " +
                                            "\"ed25519:0\": \"SomeSignatureGoesHere\"" +
                                    "}" +
                                 "}," +
                                "\"token\": \"Hello World\"" +
                          "} " +
                                "}" +
                      "]," +
                      "\"medium\": \"email\"," +
                     "\"mxid\": \"@admin:mopsdom.de\"" +
                    "}";

            var testjson = JsonNode.Parse(data);

            string url = "https://" + _config.getHomeserverHost() + ":" + Convert.ToString(_config.getHomeserverPort()) + "/_matrix/federation/v1/3pid/onbind";
                      
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(testjson);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            Console.WriteLine(httpResponse.StatusCode);

            return Ok();
        }
    }
}

