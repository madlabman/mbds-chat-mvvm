using System.Collections.ObjectModel;
using System.Linq;
using ChatApp.Client;
using ChatApp.Core;
using NLog;
using Prism.Mvvm;

namespace ChatApp.Model
{
    public class MainModel : BindableBase
    {
        private ObservableCollection<Dialog> _dialogs;
        public ReadOnlyObservableCollection<Dialog> Dialogs { get; set; }
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MainModel()
        {
            _dialogs = new ObservableCollection<Dialog>(Protocol.GetDialogs());
            foreach (var dialog in _dialogs)
            {
                UpdateDialog(dialog);
            }
            Dialogs = new ReadOnlyObservableCollection<Dialog>(_dialogs);
        }

        public void AddDialog(string login)
        {
            Logger.Info("Adding new dialog with {0}.", login);
            var dialog = Protocol.LoadDialog(login);
            if (dialog != null)
            {
                _dialogs.Add(dialog);
            }
        }

        private void UpdateDialog(Dialog dialog)
        {
            Logger.Info("Updating dialog with {0}.", dialog.Partner.Login);
            var dialogToUpdate = _dialogs.FirstOrDefault(d => d.Partner.Uuid == dialog.Partner.Uuid);
            if (dialogToUpdate != null)
            {
                dialogToUpdate.Messages = Protocol.GetDialogMessages(dialogToUpdate); // Fetching.
                dialogToUpdate.Messages = Protocol.DecryptDialogMessages(dialogToUpdate); // Decrypting.
            }
        }
    }
}