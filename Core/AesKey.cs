using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ChatApp.Core
{
    public class AesKey
    {
        public string KeyBase64 { get; set; }
        public string IvBase64 { get; set; }

        public static AesKey GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                if (aes != null)
                {
                    return new AesKey()
                    {
                        KeyBase64 = Convert.ToBase64String(aes.Key),
                        IvBase64 = Convert.ToBase64String(aes.IV)
                    };
                }
            }

            return null;
        }

        public SymmetricAlgorithm ConvertToSymmetricAlgorithm()
        {
            return new AesCryptoServiceProvider
            {
                Key = Convert.FromBase64String(KeyBase64),
                IV = Convert.FromBase64String(IvBase64)
            };
        }

        public static AesKey ConvertFromSymmetricAlgorithm(SymmetricAlgorithm key)
        {
            return new AesKey()
            {
                KeyBase64 = Convert.ToBase64String(key.Key),
                IvBase64 = Convert.ToBase64String(key.IV)
            };
        }

        public static SymmetricAlgorithm FromJsonString(string jsonString)
        {
            var aesKeyData = JsonConvert.DeserializeObject<AesKey>(jsonString);
            return aesKeyData.ConvertToSymmetricAlgorithm();
        }

        public string ToJsonString() => JsonConvert.SerializeObject(this);
    }
}