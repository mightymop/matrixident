using MatrixIdent.Models;
using Newtonsoft.Json.Linq;

namespace MatrixIdent.Services
{
    public class ConfigService
    {
        private List<PolicyItem> _pvPolicies;
        private List<PolicyItem> _termsOfServices;
        private string _pvPolicyVersion;
        private string _termsOfServiceVersion;
        private string _apiVersion;
        private string _specVersion;

        private string _ldap_domain;
        private int _ldap_port;
        private string _ldap_host;
        private string _ldap_username;
        private string _ldap_password;
        private string _ldap_filter;
        private string _ldap_searchbase;
        private string _ldap_usernameField;
        private List<string> _ldap_search_attributes;
        private int _ldap_min_search_length;

        private string _matrix_domain;

        private string _validation_mail_from;
        private string _validation_mail_subject;
        private string _validation_mail_body;
        private string _validation_mail_server;
        private int _validation_expire_after_minutes;

        private Dictionary<string, string> _ldap_3pid_map;
        private int _three_pid_expire_after_hours;

        private string _invitationMailBody;
        private string _invitationMailSubject;

        private Dictionary<string, Key> _public_keys;
        private int _ephemeral_key_expiration_hours;

        private ConfigurationManager _cmgr;

        private string _matrix_host_homeserver;
        private int _matrix_port_homeserver;

        public enum SEARCHTYPE
        {
            LDAP,
            LOCALDB
        }
        private SEARCHTYPE _searchType;

        public ConfigService(ConfigurationManager cmgr)
        {


            _cmgr = cmgr;

            _pvPolicyVersion = _cmgr.GetSection("policies").GetSection("privacy_policy").GetSection("version").Value;
            _termsOfServiceVersion = _cmgr.GetSection("policies").GetSection("terms_of_service").GetSection("version").Value;
            _apiVersion = _cmgr.GetSection("version").GetSection("api").Value;
            _specVersion = _cmgr.GetSection("version").GetSection("spec").Value;

            _pvPolicies = new List<PolicyItem>();
            _ldap_search_attributes = new List<string>();
            _public_keys = new Dictionary<string, Key>();

            _termsOfServices = new List<PolicyItem>();
            _ldap_3pid_map = new Dictionary<string, string>();

            _ldap_domain = _cmgr.GetSection("ldap").GetSection("domain").Value;
            _ldap_host = _cmgr.GetSection("ldap").GetSection("host").Value;
            _ldap_port = System.Convert.ToInt32(_cmgr.GetSection("ldap").GetSection("port").Value);
            _ldap_username = _cmgr.GetSection("ldap").GetSection("username").Value;
            _ldap_password = _cmgr.GetSection("ldap").GetSection("password").Value;
            _ldap_filter = _cmgr.GetSection("ldap").GetSection("filter").Value;
            _ldap_searchbase = _cmgr.GetSection("ldap").GetSection("searchbase").Value;
            _ldap_usernameField = _cmgr.GetSection("ldap").GetSection("usernamefield").Value;
            _ldap_min_search_length = System.Convert.ToInt32(_cmgr.GetSection("ldap").GetSection("min_search_length").Value);

            _matrix_domain = _cmgr.GetSection("matrix").GetSection("domain").Value;

            _validation_mail_from = _cmgr.GetSection("validation").GetSection("mail").GetSection("from").Value;
            _validation_mail_subject = _cmgr.GetSection("validation").GetSection("mail").GetSection("subject").Value;
            _validation_mail_body = _cmgr.GetSection("validation").GetSection("mail").GetSection("body").Value;
            _validation_mail_server = _cmgr.GetSection("validation").GetSection("mail").GetSection("server").Value;

            _validation_expire_after_minutes = Convert.ToInt32(_cmgr.GetSection("validation").GetSection("expire_after_minutes").Value);
            _three_pid_expire_after_hours = Convert.ToInt32(_cmgr.GetSection("3pid").GetSection("expire_after_hours").Value);

            _invitationMailBody = _cmgr.GetSection("invitation").GetSection("body").Value;
            _invitationMailSubject = _cmgr.GetSection("invitation").GetSection("subject").Value;

            _ephemeral_key_expiration_hours = Convert.ToInt32(_cmgr.GetSection("keys").GetSection("ephemeral").GetSection("key_expiration_hours").Value);

            foreach (ConfigurationSection itm in _cmgr.GetSection("policies").GetSection("privacy_policy").GetChildren())
            {
                if (itm.Key!="version")
                {
                    PolicyItem p = new PolicyItem(itm.Key, itm.GetSection("name").Value, itm.GetSection("url").Value);
                    _pvPolicies.Add(p);
                }
            }
            foreach (ConfigurationSection itm in _cmgr.GetSection("policies").GetSection("terms_of_service").GetChildren())
            {
                if (itm.Key != "version")
                {
                    PolicyItem p = new PolicyItem(itm.Key, itm.GetSection("name").Value, itm.GetSection("url").Value);
                    _termsOfServices.Add(p);
                }
            };
            foreach (ConfigurationSection itm in _cmgr.GetSection("ldap").GetSection("search_attributes").GetChildren())
            {
                _ldap_search_attributes.Add(itm.Value);
            };
                        
            foreach (ConfigurationSection itm in _cmgr.GetSection("3pid").GetChildren())
            {
                string key = itm.Key;
                string filter = "(|";

                foreach (ConfigurationSection itm2 in itm.GetChildren())
                {
                    filter += "(" + itm2.Value.Trim() + "=%search%*)";
                }

                if (filter=="(|")
                {
                    continue;
                }
                else
                {
                    filter += ")";
                }

                _ldap_3pid_map.Add(key, filter);
            }

            var res = Functions.getTargetFromSrvRecord(_matrix_domain,"_matrix");

            if (res != ":0")
            {
                _matrix_host_homeserver = res.Split(":")[0];
                _matrix_port_homeserver = Convert.ToInt32(res.Split(":")[1]);
            }
            else
            {
                _matrix_host_homeserver = _cmgr.GetSection("matrix").GetSection("host").Value;
                _matrix_port_homeserver = Convert.ToInt32(_cmgr.GetSection("matrix").GetSection("port").Value);
            }

            _searchType = _cmgr.GetSection("search").Value.ToLower()=="ldap"?SEARCHTYPE.LDAP:SEARCHTYPE.LOCALDB;

            Functions.addDefaultServerKeys("./appsettings.json");
        }

        public int getMinSearchLength()
        {
            return _ldap_min_search_length;
        }

        public string[] getLdapSearchAttributes()
        {
            return _ldap_search_attributes.ToArray();
        }
        public SEARCHTYPE getSearchType()
        {
            return _searchType;
        }

        public ConfigurationManager getConfigurationManager()
        {
            return _cmgr;
        }        

        public string getHomeserverHost()
        {
            return _matrix_host_homeserver;
        }

        public int getHomeserverPort()
        {
            return _matrix_port_homeserver;
        }

        public int getEphemeralKeyExpirationHours()
        {
            return _ephemeral_key_expiration_hours;
        }

        public bool checkPublicKey(string public_key)
        {
            foreach (KeyValuePair<string,Key> itm in _public_keys)
            {
                string cert = File.ReadAllText(itm.Value.public_key);

                cert = Functions.stripKeyData(cert);
                public_key = Functions.stripKeyData(public_key);

                if (cert==public_key)
                {
                    return true;
                }
            }

            return false;
        }

        public string getServerPublicKey(string keyID)
        {
            Key k = _public_keys[keyID];
            if (!keyID.StartsWith("ed225519"))
            {
                string cert = File.ReadAllText(k.public_key);
                return Functions.stripKeyData(cert);
            }
            else
            {
                return k.public_key;
            }
        }

        public string getFirstServerPublicKey()
        {
            var first = _public_keys.FirstOrDefault();
            if (!first.Key.StartsWith("ed225519"))
            {
                string cert = System.IO.File.ReadAllText(first.Value.public_key);
                return Functions.stripKeyData(cert);
            }
            else
            {
                return first.Value.public_key;
            }
        }

        public string getInvitationMailSubject()
        {
            return _invitationMailSubject;
        }

        public string getInvitationMailBody()
        {
            return _invitationMailBody;
        }

       
        public object getSignatures()
        {
            var first = _public_keys.FirstOrDefault();
            string cert = System.IO.File.ReadAllText(first.Value.public_key);
            cert = cert.Replace("-----BEGIN CERTIFICATE-----", "").Replace("-----END CERTIFICATE-----", "");
            return JObject.Parse("{\""+getMatrixDomain()+ "\": {\""+first.Key+ "\": "+ cert + "}}");
        }

        public int get3PidExpireAfterHours()
        {
            return _three_pid_expire_after_hours;
        }

        public int getValidationExpireAfterMinutes()
        {
            return _validation_expire_after_minutes;
        }

        public string getValidationMailServer()
        {
            return _validation_mail_server;
        }

        public string getValidationMailFrom()
        {
            return _validation_mail_from;
        }


        public string getValidationMailSubject()
        {
            return _validation_mail_subject;
        }


        public string getValidationMailBody()
        {
            return _validation_mail_body;
        }


        public string getMatrixDomain()
        {
            return _matrix_domain;
        }

        public Dictionary<string,string> get3PIDMap()
        {
            return _ldap_3pid_map;
        }

        public string getLDAPHost()
        {
            return _ldap_host;
        }

        public string getLDAPDomain()
        {
            return _ldap_domain;
        }

        public int getLDAPPort()
        {
            return _ldap_port;
        }

        public string getLDAPUser()
        {
            return _ldap_username;
        }

        public string getLDAPPassword()
        {
            return _ldap_password;
        }

        public string getLDAPFilter()
        {
            return _ldap_filter;
        }

        public string getLDAPSearchBase()
        {
            return _ldap_searchbase;
        }

        public string getLDAPUsernameField()
        {
            return _ldap_usernameField;
        }

        public string getPrivacyPolicyVersion()
        {
            return _pvPolicyVersion;
        }

        public string getTermsOfServiceVersion()
        {
            return _termsOfServiceVersion;
        }

        public List<PolicyItem> getPrivacyPolicies()
        {
            return this._pvPolicies;
        }

        public List<PolicyItem> getTermsOfService()
        {
            return this._termsOfServices;
        }

        public string getSpecVersion()
        {
            return _specVersion;
        }

        public string getApiVersion()
        {
            return _apiVersion;
        }
    }
}
