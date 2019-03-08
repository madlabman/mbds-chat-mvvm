using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ChatApp.Core;
using ChatApp.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace ChatApp.ViewModel
{
    public class DialogViewModel : BindableBase
    {
        private readonly DialogModel _model;
        public string DisplayName => $"{_model.Partner.Name} [{_model.Partner.Login}]";
        private ObservableCollection<MessageViewModel> _messages;
        public ReadOnlyObservableCollection<MessageViewModel> Messages { get; set; }
        public string MessageText { get; set; }
        public DelegateCommand SendMessageCommand { get; }
        public DelegateCommand UpdateDialogCommand { get; }

        public DialogViewModel(Dialog dialog)
        {
            _model = new DialogModel(dialog);
            _messages = new ObservableCollection<MessageViewModel>(
                _model.Messages.Select(m => new MessageViewModel(m)));
            Messages = new ReadOnlyObservableCollection<MessageViewModel>(_messages);
            Watch(_model.Messages, _messages);

            SendMessageCommand = new DelegateCommand(() =>
            {
                if (!string.IsNullOrEmpty(MessageText))
                {
                    _model.SendMessage(MessageText);
                    MessageText = null; // Clear message.
                    RaisePropertyChanged("MessageText");
                }
            });

            UpdateDialogCommand = new DelegateCommand(() => { _model.UpdateDialog(); });
        }

        private static void Watch<T, T2>
            (ReadOnlyObservableCollection<T> collToWatch, ObservableCollection<T2> collToUpdate)
        {
            if (collToUpdate == null) throw new ArgumentNullException(nameof(collToUpdate));
            ((INotifyCollectionChanged) collToWatch).CollectionChanged += (s, a) =>
            {
                if (a.NewItems?.Count > 0)
                {
                    foreach (var aNewItem in a.NewItems)
                    {
                        collToUpdate.Add((T2) Activator.CreateInstance(typeof(T2), (T) aNewItem));
                    }
                }

                if (a.OldItems?.Count > 0)
                {
                    foreach (var aOldItem in a.OldItems)
                    {
                        collToUpdate.Remove(collToUpdate.FirstOrDefault(item =>
                            item.GetType().GetProperty("Body")?.GetValue(item, null) ==
                            aOldItem.GetType().GetProperty("Body")?.GetValue(aOldItem, null)
                        ));
                    }
                }
            };
        }
    }
}