using IdentityModel;
using System.Security.Cryptography;
using System.Text;

namespace R10.Web.Models.IManageModels
{
    public class PkceParams
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }

        public static PkceParams GetParams()
        {
            PkceParams pkce = new PkceParams();

            var codeVerifier = CryptoRandom.CreateUniqueId(73);
            string codeChallenge;


            using (var sha256 = SHA256.Create("SHA256"))
            {
                var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                codeChallenge = Base64Url.Encode(challengeBytes);
            }

            pkce.CodeVerifier = codeVerifier;
            pkce.CodeChallenge = codeChallenge;

            return pkce;
        }
    }
}
