// Dialog object represent user to chat with and messages collection.

using System.Collections.Generic;
using System.Security.Cryptography;

namespace ChatApp.Core
{
    public class Dialog
    {
        public User Partner { get; set; }
        public List<Message> Messages { get; set; }

        public SymmetricAlgorithm Key { get; set; }

        public void LoadKey(AesKey aesKey)
        {
            Key = aesKey.ConvertToSymmetricAlgorithm();
        }
    }
}