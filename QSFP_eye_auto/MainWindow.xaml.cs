using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QSFP_eye_auto.ViewModels;

namespace QSFP_eye_auto
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly AutoTuneViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new AutoTuneViewModel();
            DataContext = _viewModel;
            cbPort.ItemsSource = _viewModel.QsfpViewModel.ComPortList;
            cbPort.SelectedIndex = 0;
        }

        private void BrdOsc_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            OscSEtting oscSEtting = new OscSEtting();
            oscSEtting.ShowDialog();
        }

        private void BrdOsa_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            OsaSetting osaSetting = new OsaSetting();
            osaSetting.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.QsfpViewModel.DisconnectFromUtbAndRefreshComPortListCommand.Execute(true);
        }

    }
}
