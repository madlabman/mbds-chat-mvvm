using System.Windows;
using ChatApp.Model;
using ChatApp.View;
using Prism.Commands;
using Prism.Mvvm;

namespace ChatApp.ViewModel
{
    public class AuthViewModel : BindableBase
    {
        private readonly AuthModel _model = new AuthModel();
        public string Name { get; set; }
        public string Login { get; set; }

        public AuthViewModel()
        {
            SignInCommand = new DelegateCommand<AuthDialog>(authDialog =>
            {
                if (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(authDialog.PasswordBox.Password))
                {
                    _model.Login = Login;
                    _model.Password = authDialog.PasswordBox.Password;
                    if (_model.SignIn())
                    {
                        // Close current window and open MainWindow.
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        authDialog.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please fill all required fields!");
                }
            });
            SignUpCommand = new DelegateCommand<AuthDialog>(authDialog =>
            {
                if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Login) &&
                    !string.IsNullOrEmpty(authDialog.PasswordBox.Password))
                {
                    _model.Name = Name;
                    _model.Login = Login;
                    _model.Password = authDialog.PasswordBox.Password;
                    if (_model.SignUp())
                    {
                        // Close current window and open MainWindow.
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        authDialog.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid credentials."); // Or no internet connection.
                    }
                }
                else
                {
                    MessageBox.Show("Please fill all required fields!");
                }
            });
        }

        public DelegateCommand<AuthDialog> SignInCommand { get; }
        public DelegateCommand<AuthDialog> SignUpCommand { get; }
    }
}