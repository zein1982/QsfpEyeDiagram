using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QsfpEyeDiagram.Controls.CdrConfiguration
{
    /// <summary>
    /// Логика взаимодействия для ThirdCdrRxControl.xaml
    /// </summary>
    public partial class ThirdCdrRxControl : QsfpEyeDiagramControlBase
    {
        public readonly Channel _channel;
        public ThirdCdrRxControl()
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
                Task.Delay(50);
            }
        }


        private void SaveSwing()
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
            SaveSwing();
        }

        private void cbMuteForce_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveSwing();
        }

        private void OutputSwingSliderRx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveSwing();
        }

        private void OutputSwingSliderRx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveSwing();
                Task.Delay(50);
            }
        }

        private void SaveDeemphasis()
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
            SaveDeemphasis();
        }

        private void DeemphasisSliderRx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveDeemphasis();
        }

        private void DeemphasisSliderRx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveDeemphasis();
                Task.Delay(50);
            }
        }

        private void SlaSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ThirdChannel.IsOperationCdrSuccessRx = false;
        }

        private void OutputSwingSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ThirdChannel.IsOperationCdrSuccessRx = false;
        }

        private void DeemphasisSliderRx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ThirdChannel.IsOperationCdrSuccessRx = false;
        }
    }
}
