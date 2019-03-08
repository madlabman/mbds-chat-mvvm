/*
 * This class know about how to sign up and sign in user, create new dialogs,
 * send messages, how to exchange and restore symmetric encryption key
 * and how to encrypt and decrypt messages.
 * So this class represent some kind of Protocol.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ChatApp.Core;

namespace ChatApp.Client
{
    public static class Protocol
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // Sign up user with passed credentials. RSA key pair will be generated automatically.
        public static bool SignUp(string name, string login, string password)
        {
            // Generate new key pair.
            var keyPair = RsaKeyPair.GenerateKeyPair();
            // Persist private key. Not the best place to do it, but it doesn't matter.
            CredentialsManager.Store(login, keyPair.PrivateKeyXml);
            // Client method call.
            return AppUser.GetInstance().Client.SignUp(login, name, password, keyPair.PublicKeyXml);
        }

        // Sign in user and restore RSA key from local storage.
        // Therefore user should be signed up on local device for successful sign in.
        // Otherwise generated private key should be transferred to a new device. 
        public static bool SignIn(string login, string password)
        {
            if (AppUser.GetInstance().Client.SignIn(login, password))
            {
                Logger.Info("Signed in as {0}.", login);
                FetchMyself(login);
                // Restore private key.
                AppUser.GetInstance().PrivateKeyXml = CredentialsManager.Retrieve(login);
                return true;
            }

            return false;
        }

        // Fetching information about current logged in user.
        private static void FetchMyself(string login)
        {
            var fetchedUser = AppUser.GetInstance().Client.FetchUser(login);
            if (fetchedUser != null)
            {
                AppUser.GetInstance().Uuid = fetchedUser.Uuid;
                AppUser.GetInstance().Login = fetchedUser.Login;
                AppUser.GetInstance().Name = fetchedUser.Name;
            }
            else
            {
                Logger.Info("Unable to fetch current user.");
            }
        }

        public static List<Dialog> GetDialogs()
        {
            var fetchedDialogs = AppUser.GetInstance().Client.GetDialogs();
            var loadedDialogs = new List<Dialog>();

            foreach (var fetchedDialog in fetchedDialogs)
            {
                loadedDialogs.Add(LoadDialog(fetchedDialog.Partner.Login));
            }

            return loadedDialogs;
        }

        // Add new dialog or fetch existing one.
        public static Dialog LoadDialog(string login)
        {
            // Fetch user data.
            var anotherUser = AppUser.GetInstance().Client.FetchUser(login);
            if (anotherUser != null)
            {
                // Construct new dialog.
                var dialog = new Dialog()
                {
                    Partner = anotherUser
                };
                dialog.Messages = GetDialogMessages(dialog);
                // Assume that if dialog exist it contains messages.
                if (dialog.Messages != null && dialog.Messages.Count > 0)
                {
                    // Try to restore dialog key.
                    TryToRestoreKeyFromDialog(ref dialog);
                    return dialog;
                }

                // Client method call.
                if (AppUser.GetInstance().Client.AddDialog(anotherUser))
                {
                    // Generate key.
                    var aesKey = AesKey.GenerateKey();
                    dialog.LoadKey(aesKey);
                    // Send message.
                    SendKeyExchangeMessage(dialog);
                    return dialog;
                }
            }

            Logger.Info("Unable to load another user.");
            return null;
        }

        private static void SendKeyExchangeMessage(Dialog dialog)
        {
            if (dialog.Key == null) return;
            // Create public key encrypted message.
            var keyMessage = new Message()
            {
                Type = MessageType.KeyExchange,
                Body = AesKey.ConvertFromSymmetricAlgorithm(dialog.Key).ToJsonString(),
                SenderUuid = AppUser.GetInstance().Uuid
            };
            EncryptMessageByAsymmetricAlgorithm(ref keyMessage, dialog.Partner.PublicKeyXml);
            AppUser.GetInstance().Client.SendMessage(dialog, keyMessage);
        }

        public static void SendMessageToTheDialog(Dialog dialog, string text)
        {
            if (dialog.Key == null) return;
            // Generate new message.
            var message = new Message()
            {
                Type = MessageType.Encrypted,
                Body = text,
                SenderUuid = AppUser.GetInstance().Uuid
            };
            EncryptMessageBySymmetricAlgorithm(ref message, dialog.Key);
            // Send message.
            AppUser.GetInstance().Client.SendMessage(dialog, message);
        }

        public static List<Message> GetDialogMessages(Dialog dialog)
        {
            return AppUser.GetInstance().Client.GetMessages(dialog) ?? new List<Message>();
        }

        public static List<Message> DecryptDialogMessages(Dialog dialog)
        {
            var decryptedMessages = new List<Message>();

            if (dialog.Key != null || TryToRestoreKeyFromDialog(ref dialog))
            {
                foreach (var dialogMessage in dialog.Messages)
                {
                    var temp = dialogMessage;
                    if (DecryptMessageBySymmetricAlgorithm(ref temp, dialog.Key))
                    {
                        decryptedMessages.Add(temp);
                    }
                }
            }

            return decryptedMessages;
        }

        // Return true if key was restored from message history.
        private static bool TryToRestoreKeyFromDialog(ref Dialog dialog)
        {
            if (dialog.Messages.Count <= 0) return false;
            // Find message in collection with type Message.ExchangeKey.
            var exchangeMessageQuery =
                (from Message m in dialog.Messages where m.Type == MessageType.KeyExchange select m);
            IEnumerable<Message> messageQueryList = exchangeMessageQuery.ToList();
            if (!messageQueryList.Any()) return false;
            var isSentByMe = false; // Change if find exchange message from current user.
            foreach (var message in messageQueryList)
            {
                if (message.SenderUuid == AppUser.GetInstance().Uuid)
                {
                    isSentByMe = true;
                    // But current user cannot decipher this message because it is encrypted by another user public key.
                }
                else
                {
                    var privateKey = AppUser.GetInstance().PrivateKeyXml;
                    // Decrypt symmetric key.
                    var temp = message; // `message` variable is immutable.
                    if (DecryptMessageByAsymmetricAlgorithm(ref temp, privateKey))
                        // Load AES key.
                        dialog.Key = AesKey.FromJsonString(temp.Body);
                }
            }

            if (dialog.Key == null) return false;
            Logger.Info("AES key was restored from dialog with {0}.", dialog.Partner.Login);
            // If message sent by current user only, there is no way to restore key.
            // Otherwise if key was restored but was not send back to another user, current user have to do that.
            if (!isSentByMe)
            {
                SendKeyExchangeMessage(dialog);
                Logger.Info("AES key was sent back to the dialog with {0}.", dialog.Partner.Login);
            }

            return true;
        }

        // Decrypt Base64Content field content and place it to the Body field.
        private static bool DecryptMessageBySymmetricAlgorithm(ref Message message, SymmetricAlgorithm key)
        {
            if (message.Type != MessageType.Encrypted) return false; // Only this type of message use AES algorithm.
            try
            {
                message.Body = Crypto.DecryptBySymmetricAlgorithm(message.Base64Content, key);
                message.Base64Content = null; // Clear message base64 content.
            }
            catch (Exception exception)
            {
                Logger.Warn("Unable to decrypt message [AES]: {0}", exception.Message);
            }
            
            return message.Body != null;
        }

        // Encrypt content from Body field and place it to the Base64Content field.
        private static bool EncryptMessageBySymmetricAlgorithm(ref Message message, SymmetricAlgorithm key)
        {
            if (message.Type != MessageType.Encrypted) return false; // Only this type of message use AES algorithm.

            try
            {
                message.Base64Content = Crypto.EncryptBySymmetricAlgorithm(message.Body, key);
                message.Body = null; // Clear message body.
            }
            catch (Exception exception)
            {
                Logger.Warn("Unable to encrypt message [AES]: {0}", exception.Message);
            }

            return message.Base64Content != null;
        }

        // Decrypt Base64Content field content and place it to the Body field.
        private static bool DecryptMessageByAsymmetricAlgorithm(ref Message message, string privateKey)
        {
            if (message.Type != MessageType.KeyExchange) return false; // Only this type of message use RSA algorithm.

            try
            {
                // Load key from XML string.
                RSA rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(privateKey);
                // Decrypt by private key.
                var cipherText = Convert.FromBase64String(message.Base64Content);
                message.Body = Encoding.UTF8.GetString(rsa.Decrypt(cipherText, RSAEncryptionPadding.Pkcs1));
                message.Base64Content = null; // Clear message base64 content.
            }
            catch (Exception exception)
            {
                Logger.Warn("Unable to decrypt message [RSA]: {0}", exception.Message);
            }

            return message.Body != null;
        }

        // Encrypt content from Body field and place it to the Base64Content field.
        private static bool EncryptMessageByAsymmetricAlgorithm(ref Message message, string publicKey)
        {
            if (message.Type != MessageType.KeyExchange) return false; // Only this type of message use RSA algorithm.

            try
            {
                // Load key from XML string.
                RSA rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(publicKey);
                var plainText = Encoding.UTF8.GetBytes(message.Body);
                message.Base64Content = Convert.ToBase64String(rsa.Encrypt(plainText, RSAEncryptionPadding.Pkcs1));
                message.Body = null; // Clear message body.
            }
            catch (Exception exception)
            {
                Logger.Warn("Unable to encrypt message [RSA]: {0}", exception.Message);
            }
            
            return message.Base64Content != null;
        }
    }
}