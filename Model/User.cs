namespace ChatApp.Model
{
    public class User
    {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string PublicKeyXml { get; set; }
        public string PrivateKeyXml { get; set; }
    }
}