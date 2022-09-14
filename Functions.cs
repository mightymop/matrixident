using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Ed25519;
using MatrixIdent.Models;
using MatrixIdent.Services;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using Stratumn.CanonicalJson;

namespace MatrixIdent
{
    public class Functions
    {
        public static string? getToken(HttpRequest req)
        {
            string auth = req.Headers.Authorization;
            if (auth == null)
            {

                Microsoft.Extensions.Primitives.StringValues strtoken;
                req.Query.TryGetValue("token", out strtoken);

                if (strtoken.Count == 0)
                {
                    return null;
                }
                else
                {
                    auth = strtoken;
                }
            }
            else
            if (auth.Trim() == "" || auth.Trim().ToLower().StartsWith("bearer") == false || auth.Trim().Split(" ").Length != 2)
            {
                return null;
            }
            else
            {
                auth = auth.Split(" ")[1];
            }

            return auth;
        }

        public static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length; // length of s
            int m = t.Length; // length of t

            if (n == 0)
            {
                return m;
            }
            else if (m == 0)
            {
                return n;
            }

            int[] p = new int[n + 1]; //'previous' cost array, horizontally
            int[] d = new int[n + 1]; // cost array, horizontally
            int[] _d; //placeholder to assist in swapping p and d

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            char t_j; // jth character of t

            int cost; // cost

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                t_j = t[j - 1];
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    cost = s[i - 1] == t_j ? 0 : 1;
                    // minimum of cell to the left+1, to the top+1, diagonally left and up +cost				
                    d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                _d = p;
                p = d;
                d = _d;
            }

            // our last action in the above loop was to switch d and p, so p now 
            // actually has the most recent cost counts
            return p[n];
        }

        public static string generateSessionID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string signJson(string json,out string private_key, out string public_key)
        {
            string strjson = System.Text.Json.JsonSerializer.Serialize(json);
            string cjson = Stratumn.CanonicalJson.Canonicalizer.Canonicalize(strjson);

            var privateKey = Ed25519.Signer.GeneratePrivateKey();
            private_key = System.Convert.ToBase64String(privateKey);
            var publicKey = privateKey.ExtractPublicKey();
            public_key = System.Convert.ToBase64String(publicKey);
            var signature = Signer.Sign(Encoding.UTF8.GetBytes(cjson), privateKey, publicKey);
            string b64 = System.Convert.ToBase64String(signature);
            return b64;
        }

        public static string signJson(object json, out string private_key, out string public_key)
        {
            string strjson = System.Text.Json.JsonSerializer.Serialize(json);
            string cjson = Stratumn.CanonicalJson.Canonicalizer.Canonicalize(strjson);

            return signJson(cjson, out private_key, out public_key);
        }

        public static string generateShortToken()
        {
            Random rnd = new Random();
            return Convert.ToString(rnd.Next(100000, 1000000));
        }

        public static string generateToken(int length)
        {
            if (length > 255)
                length = 255;
            if (length < 5)
                length = 5;

            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.=_-0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool isValidPhonenumber(string phonenumber)
        {
            if (phonenumber != null) 
                return Regex.IsMatch(phonenumber, @"^([\+]?33[-]?|[0])?[1-9][0-9]{8}$");
            else 
                return false;
        }

        public static bool isValidMail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string ConvertPrivateKeyToPem(AsymmetricKeyParameter privateKey)
        {
            using (var stringWriter = new StringWriter())
            {
                var pkcsgen = new Pkcs8Generator(privateKey);
                var pemwriter = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(stringWriter);
                pemwriter.WriteObject(pkcsgen.Generate());
                return stringWriter.ToString();
            }
        }

        private static string ConvertPublicKeyToPem(AsymmetricKeyParameter pubKey)
        {
            using (var stringWriter = new StringWriter())
            {
                var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pubKey);
                var pemWriter = new Org.BouncyCastle.Utilities.IO.Pem.PemWriter(stringWriter);
                PemObjectGenerator pemObject = new PemObject("PUBLIC KEY", publicKeyInfo.GetEncoded());
                pemWriter.WriteObject(pemObject);
                return stringWriter.ToString();
            }
        }

        public static string stripKeyData(string data)
        {
            string[] arr = data.Split("\r\n");
            data = "";
            for (int n = 0; n < arr.Length; n++)
            {
                if (!arr[n].StartsWith("---"))
                {
                    data += arr[n];
                }
            }
            return data.Trim();
        }

        public static Key createED225519Key(int expireAfterHours)
        {
            Console.WriteLine("test");
            IAsymmetricCipherKeyPairGenerator gen;
            KeyGenerationParameters param;
            gen = new Ed25519KeyPairGenerator();
            param = new Ed25519KeyGenerationParameters(new SecureRandom());
            gen.Init(param);

            var keyPair = gen.GenerateKeyPair();

            Key k = new Key();
            k.private_key = stripKeyData(ConvertPrivateKeyToPem(keyPair.Private));
            k.public_key = stripKeyData(ConvertPublicKeyToPem(keyPair.Public));
            k.expiration_timestamp = timestamp() + expireAfterHours * 60 * 60;
            k.identifier= "ed25519:"+Guid.NewGuid().ToString(); 

            return k;
        }

        public static string getTargetFromSrvRecord(string host, string service)
        {
            ProcessStartInfo nslookup_config = new ProcessStartInfo("nslookup.exe");
            nslookup_config.RedirectStandardInput = true;
            nslookup_config.RedirectStandardOutput = true;
            nslookup_config.RedirectStandardError = true;
            nslookup_config.UseShellExecute = false;
            var nslookup = Process.Start(nslookup_config);
            nslookup.StandardInput.WriteLine("set q=srv");
            nslookup.StandardInput.WriteLine(service + "._tcp." + host);
            nslookup.StandardInput.WriteLine("exit");

            string reshost = "";
            string resport = "";
            while (!nslookup.StandardOutput.EndOfStream)
            {
                string l = nslookup.StandardOutput.ReadLine();
                if (l.Contains("svr hostname"))
                {
                    while (l.Contains("\t\t"))
                    {
                        l = l.Replace("\t\t", "\t");
                    }
                    reshost = l.Substring(l.LastIndexOf("=") + 1).Trim();
                }
                if (l.Contains("port"))
                {
                    while (l.Contains("\t\t"))
                    {
                        l = l.Replace("\t\t", "\t");
                    }
                    resport = l.Substring(l.LastIndexOf("=") + 1).Trim();
                }
            }
            nslookup.Close();
            return reshost + ":" + resport;
        }

        public static void addDefaultServerKeys(string path = null)
        { 
            if (path==null)
                 path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            string fileContent = File.ReadAllText(path);
            JsonNode appsettings = JsonNode.Parse(fileContent);

            if (appsettings["keys"]==null)
            {


                Key k = createED225519Key(8);

                string json = "{" +
                                    "\"server\" : " +
                                    "{" +
                                        "\"" + k.identifier + "\" : {" +
                                            "\"public_key\": \""+k.public_key+"\"," +
                                            "\"private_key\": \""+k.private_key+"\" "+
                                        "}" +

                                    "}," +
                                    "\"ephemeral\" : " +
                                    "{" +
                                         "\"key_expiration_hours\" : 8" +
                                    "}" +
                                "}";
              


                var jsonToAdd = JsonNode.Parse(json);

               
                appsettings["keys"] = jsonToAdd;


                string output = System.Text.Json.JsonSerializer.Serialize(appsettings);

                File.WriteAllText(path, output);
            }
            
        }

        public static bool sendEmailValidationMail(string email, string url, ConfigService configService)
        {
            string body = configService.getValidationMailBody().Replace("%url%", url);

            return sendEmail(body,
            configService.getValidationMailFrom(),
            email,
            configService.getValidationMailSubject(),
            configService);           
        }

        public static bool sendInvitationMail(string address, string inviter_name, string room_name, string room_avatar, string room_type, ConfigService configService)
        {
            string body = configService.getInvitationMailBody().Replace("%1%", inviter_name).Replace("%2%", room_name).Replace("%3%", room_avatar).Replace("%4%", room_type);
            return sendEmail(body,
                      configService.getValidationMailFrom(),
                      address,
                      configService.getInvitationMailSubject(),
                      configService);
        }

        private static bool sendEmail(string body, string from, string to, string subject, ConfigService configService)
        {
            MailMessage message = new MailMessage(from, to, subject, body);
            SmtpClient client = new SmtpClient(configService.getValidationMailServer());
            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            client.UseDefaultCredentials = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in sendEmail(): {0}",
                    ex.ToString());
            }
            return true;
        }

        public static bool sendPhondeValidationSMS(string phonenumber, string token, ConfigService configService)
        {
            return false; // NOT IMPLEMENTED
        }

        public static long timestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}
