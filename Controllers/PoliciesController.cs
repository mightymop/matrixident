using MatrixIdent.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using MatrixIdent.Models;
using log4net;

namespace MatrixIdent.Controllers
{
    [Route("_matrix/identity/v2/terms")]
    [ApiController]
    public class PoliciesController : ControllerBase
    {
        private ConfigService _config;
        private ILog log = LogManager.GetLogger(typeof(PoliciesController));

        public PoliciesController(ConfigService config)
        {
            this._config = config;
        }

        private object buildPrivacyPolicyObject()
        {
            List<PolicyItem> pvItems = _config.getPrivacyPolicies();

            var dictionary = new Dictionary<string, object>();
            foreach (PolicyItem itm in pvItems)
            {
                dictionary.Add(itm.language, itm.toObject());
            }
            dictionary.Add("version", this._config.getPrivacyPolicyVersion());
            return dictionary;
        }

        private object buildTermsOfServiceObject()
        {
            List<PolicyItem> tItems = _config.getTermsOfService();

            var dictionary = new Dictionary<string, object>();
            foreach (PolicyItem itm in tItems)
            {
                dictionary.Add(itm.language, itm.toObject());
            }

            dictionary.Add("version", this._config.getTermsOfServiceVersion());
            return dictionary;
        }

        private object buildPoliciesObject()
        {
            /*
             {
                  "policies": {
                    "privacy_policy": {
                      "en": {
                        "name": "Privacy Policy",
                        "url": "https://example.org/somewhere/privacy-1.2-en.html"
                      },
                      "fr": {
                        "name": "Politique de confidentialité",
                        "url": "https://example.org/somewhere/privacy-1.2-fr.html"
                      },
                      "version": "1.2"
                    },
                    "terms_of_service": {
                      "en": {
                        "name": "Terms of Service",
                        "url": "https://example.org/somewhere/terms-2.0-en.html"
                      },
                      "fr": {
                        "name": "Conditions d'utilisation",
                        "url": "https://example.org/somewhere/terms-2.0-fr.html"
                      },
                      "version": "2.0"
                    }
                  }
                }
             
             */
            
            return new { policies = new { privacy_policy = buildPrivacyPolicyObject(), terms_of_service = buildTermsOfServiceObject() } };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(buildPoliciesObject());
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Post()
        {
            return Ok(new { });
        }
    }
}

