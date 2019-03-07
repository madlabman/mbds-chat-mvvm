using ChatApp.Client;

namespace ChatApp.Model
{
    public class AuthModel
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public bool SignIn()
        {
            return Protocol.SignIn(Login, Password);
        }

        public bool SignUp()
        {
            return Protocol.SignUp(Name, Login, Password);
        }
    }
}