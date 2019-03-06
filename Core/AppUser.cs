using System.Security.Cryptography;

namespace ChatApp.Core
{
    // Create instance of current application user and use for access across application.
    public class AppUser : User
    {
        private static AppUser _user;

        public Client.Client Client { get; set; }

        private AppUser()
        {
            // TODO: Persist user data.
            Client = new Client.Client();
        }

        public static AppUser GetInstance()
        {
            return _user ?? (_user = new AppUser());
        }
    }
}