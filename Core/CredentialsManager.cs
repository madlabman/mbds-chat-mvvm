// Plaintext files as private keys storage.

namespace ChatApp.Core
{
    public static class CredentialsManager
    {
        public static void Store(string login, string key)
        {
            System.IO.File.WriteAllText(login + "-private-key.xml", key);
        }

        public static string Retrieve(string login)
        {
            return System.IO.File.ReadAllText(login + "-private-key.xml");
        }
    }
}