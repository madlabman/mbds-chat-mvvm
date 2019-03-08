using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using ChatApp.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace ChatApp.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private readonly MainModel _model;
        private ObservableCollection<DialogViewModel> _dialogs;
        public ReadOnlyObservableCollection<DialogViewModel> Dialogs { get; set; }
        public string LoginToAddDialog { get; set; }
        public DelegateCommand AddDialogCommand { get; }

        public MainViewModel()
        {
            _model = new MainModel();
            _dialogs = new ObservableCollection<DialogViewModel>(_model.Dialogs.Select(
                d => new DialogViewModel(d)));
            Dialogs = new ReadOnlyObservableCollection<DialogViewModel>(_dialogs);
            Watch(_model.Dialogs, _dialogs, model => model.Messages);

            AddDialogCommand = new DelegateCommand(() =>
            {
                if (!string.IsNullOrEmpty(LoginToAddDialog))
                {
                    _model.AddDialog(LoginToAddDialog);
                    LoginToAddDialog = null; // Empty field.
                }
            });
        }

        private static void Watch<T, T2>
        (ReadOnlyObservableCollection<T> collToWatch, ObservableCollection<T2> collToUpdate,
            Func<T2, object> modelProperty)
        {
            if (collToUpdate == null) throw new ArgumentNullException(nameof(collToUpdate));
            ((INotifyCollectionChanged) collToWatch).CollectionChanged += (s, a) =>
            {
                if (a.NewItems?.Count == 1)
                    collToUpdate.Add((T2) Activator.CreateInstance(typeof(T2), (T) a.NewItems[0]));
            };
        }
    }
}