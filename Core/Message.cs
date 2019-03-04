using System;
using System.Security.Cryptography;
using System.Text;
using ChatApp.Helper;
using Newtonsoft.Json;

namespace ChatApp.Core
{
    public enum MessageType
    {
        Undefined = 0, // Default value when deserialize object.
        KeyExchange, // Indicate key exchange procedure.
        Plaintext, // Normal message.
        Encrypted // Secured with AES.
    }

    public class Message
    {
        public MessageType Type { get; set; }

        public string SenderUuid { get; set; }

        public string Base64Content { get; set; } // Everyone message contain base64-encoded encrypted text.

        [JsonIgnore] // No need for serialization / deserialization.
        public string Body => _decrypted;
        private string _decrypted = null;

        // Decrypt Base64Content field content and place it to the Body field.
        public bool DecryptBodyBySymmetricAlgorithm(SymmetricAlgorithm key)
        {
            if (Type != MessageType.Encrypted) return false; // Only this type of message use AES algorithm.
            _decrypted = CryptoHelper.DecryptBySymmetricAlgorithm(Base64Content, key);
            return _decrypted != null;
        }

        // Encrypt content from Body field and place it to the Base64Content field.
        public bool EncryptBodyBySymmetricAlgorithm(SymmetricAlgorithm key)
        {
            if (Type != MessageType.Encrypted) return false; // Only this type of message use AES algorithm.
            Base64Content = CryptoHelper.EncryptBySymmetricAlgorithm(Body, key);
            return Base64Content != null;
        }

        // Decrypt Base64Content field content and place it to the Body field.
        public bool DecryptBodyByAsymmetricAlgorithm(string privateKey)
        {
            if (Type != MessageType.KeyExchange) return false; // Only this type of message use RSA algorithm.
            // Load key from XML string.
            RSA rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            // Decrypt by private key.
            var cipherText = Convert.FromBase64String(Base64Content);
            _decrypted = Encoding.UTF8.GetString(rsa.Decrypt(cipherText, RSAEncryptionPadding.Pkcs1));
            return true; // It will crash at any other way :)
        }

        // Encrypt content from Body field and place it to the Base64Content field.
        public bool EncryptBodyByAsymmetricAlgorithm(string publicKey)
        {
            if (Type != MessageType.KeyExchange) return false; // Only this type of message use RSA algorithm.
            // Load key from XML string.
            RSA rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            var plainText = Encoding.UTF8.GetBytes(Body);
            Base64Content = Convert.ToBase64String(rsa.Encrypt(plainText, RSAEncryptionPadding.Pkcs1));
            return true;
        }

        // For simplicity keep the same format across server and client side and serialize and deserialize implicitly
        public static Message FromJsonString(string jsonString) => JsonConvert.DeserializeObject<Message>(jsonString);
        public string ToJsonString() => JsonConvert.SerializeObject(this);
    }
}