using System;
using System.IO;
using System.Security.Cryptography;

namespace ChatApp.Core
{
    public class Crypto
    {
        public static string DecryptBySymmetricAlgorithm(string base64CipherText, SymmetricAlgorithm aesAlgorithm)
        {
            var cipherText = Convert.FromBase64String(base64CipherText);
            string plainText = null;

            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netframework-4.7.2
            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);
            // Create the streams used for decryption.
            using (var msDecrypt = new MemoryStream(cipherText))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream and place them in a string.
                        plainText = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plainText.Length <= 0 ? null : plainText;
        }

        // Return result in base64-encoded form
        public static string EncryptBySymmetricAlgorithm(string plainText, SymmetricAlgorithm aesAlgorithm)
        {
            byte[] encrypted;
            // Create an encryptor to perform the stream transform.
            var encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);
            // Create the streams used for encryption.
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }
            // Return encoded data.
            return Convert.ToBase64String(encrypted);
        }
    }
}