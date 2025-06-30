using QsfpEyeDiagram.ViewModels;
using Std.Data.Database.Domain;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using QsfpEyeDiagram;
using Std.Modules.ConfigurationParameters.Qsfp;
using System.Windows.Controls;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QsfpEyeDiagram
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly double wlIdeal1 = 1295.56;
        public static readonly double wlIdeal2 = 1300.05;
        public static readonly double wlIdeal3 = 1304.58;
        public static readonly double wlIdeal4 = 1309.14;

        public const double wlMin1 = 1294.53;
        public const double wlMin2 = 1299.02;
        public const double wlMin3 = 1303.54;
        public const double wlMin4 = 1308.09;

        public const double wlMax1 = 1296.59;
        public const double wlMax2 = 1301.09;
        public const double wlMax3 = 1305.53;
        public const double wlMax4 = 1310.19;

        private readonly QsfpEyeDiagramViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = (QsfpEyeDiagramViewModel)DataContext;
            ComPortComboBox.SelectedIndex = 0;
        }

        public MainWindow(WorkerRecord worker) : this()
        {
            _viewModel.Worker = worker;
        }

        private void WriteTecOptimalTemperatureVoltage(object sender, MouseButtonEventArgs e)
        {
            _viewModel.WriteTecOptimalTemperatureVoltageCommand.Execute(new object[] { _viewModel.TecOptimalTemperatureVoltage });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _viewModel.QuitCommand.Execute(null);
        }

        private void Peak1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var control = sender as TextBox;
            if (Double.TryParse(control.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubValue))
            {
                if (doubValue > wlMin1 && doubValue < wlMax1)
                    control.Foreground = Brushes.Green;
                else
                    control.Foreground = Brushes.Red;
            }
            else control.Text = "N/A";
        }

        private void Peak2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var control = sender as TextBox;
            if (Double.TryParse(control.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubValue))
            {
                if (doubValue > wlMin2 && doubValue < wlMax2)
                    control.Foreground = Brushes.Green;
                else
                    control.Foreground = Brushes.Red;
            }
            else control.Text = "N/A";
        }

        private void Peak3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var control = sender as TextBox;
            if (Double.TryParse(control.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubValue))
            {
                if (doubValue > wlMin3 && doubValue < wlMax3)
                    control.Foreground = Brushes.Green;
                else
                    control.Foreground = Brushes.Red;
            }
            else control.Text = "N/A";
        }

        private void Peak4_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var control = sender as TextBox;
            if (Double.TryParse(control.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubValue))
            {
                if (doubValue > wlMin4 && doubValue < wlMax4)
                    control.Foreground = Brushes.Green;
                else
                    control.Foreground = Brushes.Red;
            }
            else control.Text = "N/A";
        }

        private void TbTemperature_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TecSlider.Value = Convert.ToDouble(TbTemperature.Text);
                _viewModel.WriteTecOptimalTemperatureVoltageCommand.Execute(new object[] { _viewModel.TecOptimalTemperatureVoltage });
                TecSlider.Focus();
            }
        }


        private void btnAutoTecTune_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(_viewModel!=null &&_viewModel.IsModuleConnected)
            {
                TecSlider.IsEnabled = btnAutoTecTune.IsEnabled;
                btnAutoTecTune.InvalidateVisual();

            }
        }

        private void TecSlider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if((e.Key==Key.Left) || (e.Key == Key.Right))
            {
                _viewModel.WriteTecOptimalTemperatureVoltageCommand.Execute(new object[] { _viewModel.TecOptimalTemperatureVoltage });
            }
        }


        private void miSettings_Click(object sender, RoutedEventArgs e)
        {
            //tiSettings.Visibility = Visibility.Visible;
            tiSettings.IsSelected = true;
        }

        private void tbSigma_LostFocus(object sender, RoutedEventArgs e)
        {
            double localsigma;
            TextBox tb = sender as TextBox;
            string text = tb.Text.Replace(',', '.');
            tb.Text = text;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out localsigma))
            {
                Properties.Settings.Default.sigma = localsigma;
                Properties.Settings.Default.Save();
            }
            else tb.Text = Properties.Settings.Default.sigma.ToString();

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                if ((sender as ComboBox).SelectedIndex == 0)
                {
                    if(_viewModel.bertTimer != null)
                        _viewModel.bertTimer.Stop();
                    _viewModel.BertwaveIpAddress = Properties.Settings.Default.BertwaveIP1;
                }
                else
                {
                    _viewModel.BertwaveIpAddress = Properties.Settings.Default.BertwaveIP;
                }
                if (_viewModel.IsBertwaveConnected)
                {
                    _viewModel.DisconnectFromBertwave();
                    //_viewModel.ConnectToBertwave(_viewModel.BertwaveIpAddress);
                }
            }
        }

        private void tbOscylIP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Is4C)
            {
                if (tbOscylIP.Text != Properties.Settings.Default.BertwaveIP)
                {
                    Properties.Settings.Default.BertwaveIP = tbOscylIP.Text;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                if (tbOscylIP.Text != Properties.Settings.Default.BertwaveIP1)
                {
                    Properties.Settings.Default.BertwaveIP1 = tbOscylIP.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           if (!_viewModel.IsModuleTuneEnded)
           {
               if (_viewModel.IsUidNotNull)
               {
                   MessageBoxResult res = MessageBox.Show("Перед выходом из программы завершите настройку модуля кнопкой 'Завершить'", "Настройка не завершена", MessageBoxButton.OK, MessageBoxImage.Stop);
                   e.Cancel = true;
               }
           }
        }

        private void tbOsaIP_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tbOsaIP.Text != Properties.Settings.Default.OSAIP)
            {
                Properties.Settings.Default.OSAIP = tbOsaIP.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
