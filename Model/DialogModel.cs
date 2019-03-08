using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ChatApp.Client;
using ChatApp.Core;
using NLog;
using Prism.Mvvm;

namespace ChatApp.Model
{
    public class DialogModel : BindableBase
    {
        private Dialog _dialog;
        private ObservableCollection<Message> _messages;
        public ReadOnlyObservableCollection<Message> Messages { get; set; }
        public User Partner => _dialog.Partner;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public DialogModel(Dialog dialog)
        {
            _dialog = dialog;
            _messages = new ObservableCollection<Message>(dialog.Messages);
            Messages = new ReadOnlyObservableCollection<Message>(_messages);
        }

        public void SendMessage(string message)
        {
            Logger.Info("Sending message to {0}.", _dialog.Partner.Login);
            Protocol.SendMessageToTheDialog(_dialog, message);
            UpdateDialog();
        }

        public void UpdateDialog()
        {
            Logger.Info("Updating dialog with {0}.", _dialog.Partner.Login);
            _dialog.Messages = Protocol.GetDialogMessages(_dialog); // Fetching.
            _dialog.Messages = Protocol.DecryptDialogMessages(_dialog); // Decrypting.
            
            while (_messages.Count > 0)
            {
                _messages.RemoveAt(0);
            }

            foreach (var message in _dialog.Messages)
            {
                _messages.Add(message);
            }
        }
    }
}