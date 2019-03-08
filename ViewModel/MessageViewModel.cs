using System.Windows;
using ChatApp.Core;
using Prism.Mvvm;

namespace ChatApp.ViewModel
{
    public class MessageViewModel : BindableBase
    {
        private readonly Message _message;
        public string Body { get; set; }
        public bool IsSentByMe => _message.IsSentByMe;

        public Visibility IsVisible // Show only decrypted with RSA messages.
        {
            get
            {
                if (_message.Type == MessageType.Encrypted && _message.Body != null)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }
        
        public MessageViewModel(Message message)
        {
            _message = message;
            Body = message.Body;
        }
    }
}