using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
using System.Threading;
using System.Windows.Input;

namespace QsfpEyeDiagram.Controls.CdrConfiguration
{
    /// <summary>
    /// Логика взаимодействия для ZeroCdrRxControl.xaml
    /// </summary>
    public partial class ZeroCdrRxControl : QsfpEyeDiagramControlBase
    {
        public readonly Channel _channel;
        public ZeroCdrRxControl()
        {
            InitializeComponent();
            _channel = (Channel)Resources["Channel"];
        }


        private void SaveCdrSla()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

            ViewModel.UtbMonitoringPause();
            ViewModel.WriteSla((bool)SlaEnRx.IsChecked, (int)SlaSliderRx.Value, _channel);
            ViewModel.UtbMonitoringRelease();

            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }
        }

        private void SlaSliderRx_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SaveCdrSla();
        }

        private void SlaEnRx_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveCdrSla();
        }

        private void SlaSliderRx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveCdrSla();
                
            }
        }

        private void SaveCdrSwing()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
            ViewModel.UtbMonitoringPause();

            ViewModel.WriteCdrSwing((bool)cbMuteForce.IsChecked, (bool)cbDisAutoMute.IsChecked, (byte)OutputSwingSliderRx.Value, _channel, 0);

            ViewModel.UtbMonitoringRelease();
            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }
        }


        private void cbDisAutoMute_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveCdrSwing();
        }

        private void cbMuteForce_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveCdrSwing();
        }

        private void OutputSwingSliderRx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCdrSwing();
        }

        private void OutputSwingSliderRx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveCdrSwing();
            }
        }


        private void SaveDeephasis()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
            ViewModel.UtbMonitoringPause();

            ViewModel.WriteCdrDeemphasis((bool)cbSlowSlew.IsChecked, (byte)DeemphasisSliderRx.Value, _channel, 0);
            ViewModel.UtbMonitoringRelease();
            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }
        }

        private void cbSlowSlew_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveDeephasis();
        }

        private void DeemphasisSliderRx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveDeephasis();
        }

        private void DeemphasisSliderRx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveDeephasis();
            }
        }

        private void DeemphasisSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessRx = false;
        }

        private void OutputSwingSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessRx = false;
        }

        private void SlaSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessRx = false;
        }
    }
}
