// Dialog object represent user to chat with and messages collection.

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ChatApp.Core
{
    public class Dialog
    {
        public User Partner { get; set; }
        public List<Message> Messages { get; set; }

        private SymmetricAlgorithm _key;

        // Return true if key was restored from message history.
        private bool _restoreKeyFromMessageHistory() 
        {
            if (_key != null || Messages.Count <= 0) return false;
            // Find message in collection with type Message.ExchangeKey.
            var exchangeMessageQuery =
                (from Message m in Messages where m.Type == MessageType.KeyExchange select m);
            IEnumerable<Message> messageQueryList = exchangeMessageQuery.ToList();
            if (!messageQueryList.Any()) return false;
            var message = messageQueryList.First();
            if (message.Base64Content == null) return false;
            var privateKey = message.SenderUuid == AppUser.GetInstance().Uuid
                ? AppUser.GetInstance().PrivateKeyXml
                : Partner.PrivateKeyXml;
            // Decrypt symmetric key.
            if (!message.DecryptBodyByAsymmetricAlgorithm(privateKey)) return false;
            // Load AES key.
            _key = AesKey.FromJsonString(message.Body);
            return true;
        }
    }
}