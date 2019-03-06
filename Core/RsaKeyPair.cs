using System.Security.Cryptography;

namespace ChatApp.Core
{
    public class RsaKeyPair
    {
        public string PublicKeyXml { get; set; }
        public string PrivateKeyXml { get; set; }

        public static RsaKeyPair GenerateKeyPair()
        {
            RSA rsa = new RSACryptoServiceProvider(2048);
            return new RsaKeyPair()
            {
                PublicKeyXml = rsa.ToXmlString(false),
                PrivateKeyXml = rsa.ToXmlString(true)
            };
        }
    }
}