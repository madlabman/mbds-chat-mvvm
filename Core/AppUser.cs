using System.Security.Cryptography;

namespace ChatApp.Core
{
    // Create instance of current application user and use for access across application.
    public class AppUser : User
    {
        private static AppUser _user;

        public AppUser()
        {
            // TODO: Move to registration.
            RSA rsa = new RSACryptoServiceProvider(2048);
            // Export generated key parameters.
            PublicKeyXml = rsa.ToXmlString(false);
            PrivateKeyXml = rsa.ToXmlString(true);
            // TODO: Persist user data.
        }

        public static AppUser GetInstance()
        {
            return _user ?? (_user = new AppUser());
        }
    }
}