using System;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace ChatApp.Model
{
    public class AesKey
    {
        public string KeyBase64 { get; set; }
        public string IvBase64 { get; set; }

        public static SymmetricAlgorithm FromJsonString(string jsonString)
        {
            var aesKeyData = JsonConvert.DeserializeObject<AesKey>(jsonString);
            return new AesCryptoServiceProvider
            {
                Key = Convert.FromBase64String(aesKeyData.KeyBase64),
                IV = Convert.FromBase64String(aesKeyData.IvBase64)
            };
        }

        public string ToJsonString() => JsonConvert.SerializeObject(this);
    }
}