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
        public string Body { get; set; }

        // For simplicity keep the same format across server and client side and serialize and deserialize implicitly
        public static Message FromJsonString(string jsonString) => JsonConvert.DeserializeObject<Message>(jsonString);
        public string ToJsonString() => JsonConvert.SerializeObject(this);
    }
}