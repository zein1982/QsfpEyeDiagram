using System.IO;
using System.Windows;

namespace QsfpEyeDiagram
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            UserName.Text = Properties.Settings.Default.lastUser;
            UserName.Focus();
        }

    }
}
