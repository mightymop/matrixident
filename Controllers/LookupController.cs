using Microsoft.AspNetCore.Mvc;
using MatrixIdent.Models;
using MatrixIdent.Services;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.Protocols;
using System.Text.Json.Nodes;
using System.Collections;
using System.Net;

using log4net;


namespace MatrixIdent.Controllers
{
    [Route("/_matrix/identity/v2")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly DBService _dbService;
        private readonly LDAPService _ldapService;
        private readonly ConfigService _configService;

        private ILog log = LogManager.GetLogger(typeof(LookupController));


        public LookupController(DBService dbService, LDAPService ldapService, ConfigService configService)
        {
            _dbService = dbService;
            _ldapService = ldapService;
            _configService = configService;
        }


        [HttpGet("/_matrix/identity/v2/hash_details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> hash_details()
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

            string lookup_pepper = _configService.getSearchType()==ConfigService.SEARCHTYPE.LOCALDB?Functions.generateToken(20):"x";
            if (await _dbService.addOrUpdateHash(auth, lookup_pepper))
            {
                string[] algos = _configService.getSearchType() == ConfigService.SEARCHTYPE.LOCALDB ? new[] { "none", "sha256" } : new[] {"none"} ;
                var result = new { algorithms = algos, lookup_pepper = lookup_pepper };

                return Ok(result);
            }
            else
            {
                return Problem(new ErrorType("DATABASE ERROR", "Could not update / insert data (in)to database.").ToString());
            }
        }


        [HttpPost("/_matrix/identity/v2/lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> lookup(LookupRequest request)
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

            if (request.pepper==null)
            {
                return BadRequest(new ErrorType("M_INVALID_PEPPER", "Unknown or invalid pepper - has it been rotated?").ToJSON());
            }

            if (request.algorithm=="none")
            {
                //LDAP
                return await processLDAP(request);
            }
            else
            {
                if (request.algorithm=="sha256")
                {
                    return await processRainbowTable(request);
                }
                else
                {
                    return BadRequest(new ErrorType("M_NOT_IMPLEMENTED","The algoritm is not supported by the server").ToJSON());
                }
            }
          
        }

        public class ResultComparator: IComparer
        {
            private string _search;

            public ResultComparator(string search)
            {
                this._search = search;
            }
                
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                int scroreX = Functions.ComputeLevenshteinDistance(_search, ((LdapResult)x).displayName);
                int scroreY = Functions.ComputeLevenshteinDistance(_search, ((LdapResult)y).displayName);
                if (scroreX < scroreY)
                    return -1;
                if (scroreX > scroreY)
                    return 1;

                return ((LdapResult)x).displayName.CompareTo(((LdapResult)y).displayName);
            }
        }


        private async Task<IActionResult> lookupDirectory(SearchRequestItem request)
        {
            string token = Functions.getToken(Request);
            if (token == null)
            {
                return Unauthorized(new ErrorType("M_NOT_AUTHORIZED", "THIS ENDPOINT NEEDS AUTHORIZATION FIRST.").ToJSON());
            }

            //Terms checken
            if (!true)
            {
                return Forbid(new ErrorType("M_TERMS_NOT_SIGNED", "YOU NEED TO ACCEPT THE TERMS FIRST.").ToString());
            }

            if (request.search_term.Length<_configService.getMinSearchLength())
            {
                //return BadRequest(ErrorType.M_INVALID_PARAM.ToJSON());
                return new ObjectResult(new
                {
                    errcode= "M_LIMIT_EXCEEDED",
                    error = "Search query to short",
                    retry_after_ms= 3000
                })
                {
                    StatusCode = (int?)HttpStatusCode.TooManyRequests
                };
            }

            try
            {
                LdapResult[] resultarray = await _ldapService.lookup2(request.search_term);
                IComparer myComparer = new ResultComparator(request.search_term);
                Array.Sort(resultarray, myComparer);
                if (resultarray != null && resultarray.Length > 0)
                {
                    int max;
                    if (request.limit != null && resultarray.Length > request.limit)
                    {
                        max = (int)request.limit;
                    }
                    else
                    {
                        max = resultarray.Length;
                    }

                    List<object> results = new List<object>();
                    for (int n=0;n<max; n++)
                    {
                        results.Add(new { display_name = resultarray[n].displayName, user_id = resultarray[n].id});
                    }

                    var result = new { limited = max < resultarray.Length ? true : false, results = results.ToArray() };
                    return Ok(result);
                }
                else
                {
                    return Ok(new { limited = false, results = new object[] {} });
                }
            }
            catch (LdapException e)
            {
                //    log.Error("user data not collected: " + e.Message, e);
                return Problem(new ErrorType("M_LDAP_ERROR", e.Message).ToString());
            }
            catch (Exception e2)
            {
                //    log.Error("user data not collected: " + e2.Message, e2);
                return Problem(new ErrorType("M_UNKNOWN", e2.Message).ToString());
            }

        }


        [HttpPost("/_matrix/client/r0/user_directory/search")]
        [HttpPost("/_matrix/client/v3/user_directory/search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> lookup(SearchRequestItem request)
        {
             return await lookupDirectory(request);
        }

        private async Task<IActionResult> processLDAP(LookupRequest request)
        {
            var mappings = JsonNode.Parse("{}");

            foreach (string address in request.addresses)
            {
                string[] parts = address.Split(" ");
                string search = parts[0];
                string medium = parts[1];

                try
                {
                    LdapResult[] resultarray = await _ldapService.lookup(search, medium);
                    if (resultarray!=null&& resultarray.Length>0)
                    {
                        mappings[address] = "@" + resultarray[0].id + ":" + _configService.getLDAPDomain();
                    }
                }
                catch (LdapException e)
                {
                    //    log.Error("user data not collected: " + e.Message, e);
                    return Problem(new ErrorType("M_LDAP_ERROR",e.Message).ToString());
                }
                catch (Exception e2)
                {
                    //    log.Error("user data not collected: " + e2.Message, e2);
                    return Problem(new ErrorType("M_UNKNOWN", e2.Message).ToString());
                }
            }

            return  Ok(new { mappings });
        }
        

        private async Task<IActionResult> processRainbowTable(LookupRequest request)
        {
            var mappings = JObject.Parse("{}");

            var all3Pids = await _dbService.get3PidItems();
                            
            for (int i=0;i<all3Pids.Length;i++)
            {

                foreach (string address in request.addresses)
                {
                    string test = all3Pids[i].address + " " + all3Pids[i].medium+" "+request.pepper;
                    if (Crypt.sha256(test) == address)
                    {
                        mappings.Add(address, all3Pids[i].mxid);
                    }
                }
            }

            var result = new { mappings = mappings };

            return Ok(result);
        }
    }
}

