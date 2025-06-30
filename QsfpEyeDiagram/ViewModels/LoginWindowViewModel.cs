using QsfpEyeDiagram.Models;
using QsfpEyeDiagram.ViewModels.Commands;
using System.Windows;
using System.Windows.Input;

namespace QsfpEyeDiagram.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        public LoginWindow LoginWindow { get; set; }

        private readonly LoginWindowModel _model;

        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    OnPropertyChanged();
                }
            }
        }

        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                    (_loginCommand = new RelayCommand(
                        p => !string.IsNullOrWhiteSpace(Login),
                        p => LoginToApplication(Login)));
            }
        }

        public LoginWindowViewModel()
        {
            _model = new LoginWindowModel();
        }

        private void ExitApp()
        {
            _model.CloseLoginWindow();
        }

        private void LoginToApplication(string login)
        {
            Properties.Settings.Default.lastUser = login;
            Properties.Settings.Default.Save();

            var result = _model.LoginToApplication(login, PasswordHelper.PasswordString);

            if (result.Status != OperationStatuses.Success)
            {
                MessageBox.Show(result.GetAllMessages(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private ICommand _exitCommand;
        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(
                p => true,
                p => ExitApp()
                ));
    }
}
