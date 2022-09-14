
using MatrixIdent.Models;
using System.Collections;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace MatrixIdent.Services
{
    public class LDAPService
    {

        private ConfigService _configService;


        public LDAPService(ConfigService config)
        {
            this._configService = config;
        }

        private string getFilterPartFromMedium(string medium)
        {
            return _configService.get3PIDMap().GetValueOrDefault(medium);
        }

        private string prepareFilter2(string search, string[] ldapAttributes)
        {
            string searchfilter = "(|";
            for (int n = 0; n < ldapAttributes.Length; n++)
            {
                string[] arr = search.Split(" ");
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].Trim().Length > 0)
                    {
                        searchfilter += "(" + ldapAttributes[n] + "=" + arr[i].Trim() + "*)";
                    }
                }
            }

            searchfilter += ")";

            return "(&" + _configService.getLDAPFilter() + searchfilter + ")";
        }

        private string prepareFilter1(string search, string medium)
        {
            string searchfilter = getFilterPartFromMedium(medium).Replace("%search%", search);
            return "(&" + _configService.getLDAPFilter() + searchfilter + ")";
        }

        public LdapResult[] searchLdap(string ldapFilter)
        {
            // log.Debug("get User");
            List<LdapResult> res = new List<LdapResult>();
            // LoginResult res = new LoginResult();

            // log.Debug("empty UserInfo created");

            List<string> userprops = new List<string>();
            userprops.AddRange(new string[] { _configService.getLDAPUsernameField(),"displayName" });
            //log.Debug("attributes defined");

            // log.Debug("set SearchRequest");

            LdapConnection connection = connect(_configService.getLDAPUser(), _configService.getLDAPPassword());
            SearchRequest searchRequest2 = new SearchRequest(_configService.getLDAPSearchBase(), ldapFilter, System.DirectoryServices.Protocols.SearchScope.Subtree, userprops.ToArray());

            //     log.Debug("get SearchResponse");
            SearchResponse response = (SearchResponse)connection.SendRequest(searchRequest2);

            //   log.Debug("parse Entries");
            string usernamefield = _configService.getLDAPUsernameField().ToLower();
            foreach (SearchResultEntry entry in response.Entries)
            {

                LdapResult result = new LdapResult();

                foreach (string attributeName in entry.Attributes.AttributeNames)
                {
                    string name = attributeName.ToLower();
                    //        log.Debug((string)entry.Attributes[attributeName][0]);
                    if (name.Equals(usernamefield))
                    {
                        result.id = (string)(entry.Attributes[attributeName][0]);
                    }
                    if (name.Equals("displayname"))
                    {
                        result.displayName = (string)(entry.Attributes[attributeName][0]);
                    }

                }

                res.Add(result);
            }

            return res.ToArray();
        }

        public async Task<LdapResult[]> lookup(string search, string medium)
        {
            string ldapFilter = prepareFilter1(search, medium);
            return searchLdap(ldapFilter);
        }

        public async Task<LdapResult[]> lookup2(string search)
        {
            string ldapFilter = prepareFilter2(search, _configService.getLdapSearchAttributes());
            return searchLdap(ldapFilter);
        }

        private LdapConnection connect(string user, string pass)
        {
            try
            {
                return connectHelper(user, pass, _configService.getLDAPHost());
            }
            catch (LdapException e)
            {
                return connectHelper(user, pass, _configService.getLDAPDomain());
            }
        }

        private LdapConnection connectHelper(string user, string pass, string host)
        {
            int port = _configService.getLDAPPort();

            LdapConnection connection = new LdapConnection(host);

            connection.AuthType = AuthType.Ntlm;
            connection.SessionOptions.VerifyServerCertificate += (LdapConnection ldapCon, X509Certificate cert) =>
            {
                // log.Debug("Cert: Issuer=" + cert.Issuer + " Subject=" + cert.Subject);
                return true;
            };
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            connection.SessionOptions.ProtocolVersion = 3;
            connection.SessionOptions.SecureSocketLayer = port == 636 ? true : false;
            //    log.Debug("HOST reachable: " + (connection.SessionOptions.HostReachable ? "true" : "false"));
            var networkCredential = new NetworkCredential(user, pass);
            connection.Bind(networkCredential);

            return connection;
        }


    }
}
