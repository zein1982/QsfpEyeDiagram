using System.Threading.Tasks;
using System.Windows.Input;
using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Cdr;
using QsfpEyeDiagram.ViewModels;

namespace QsfpEyeDiagram.Controls.CdrConfiguration
{
    /// <summary>
    /// Логика взаимодействия для ZeroCdrTxControl.xaml
    /// </summary>
    public partial class ZeroCdrTxControl : QsfpEyeDiagramControlBase
    {

        public readonly Channel _channel;
        public ZeroCdrTxControl()
        {
            InitializeComponent();
            _channel = (Channel)Resources["Channel"];
            
        }

        private void SaveCDrCtle()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
            
            ViewModel.UtbMonitoringPause();
            ViewModel.WriteCdrCtle((byte)CtleSliderTx.Value, _channel);
            ViewModel.UtbMonitoringRelease();

            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }
        }

        private void CtleSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCDrCtle();
        }

        private void CtleSliderTx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveCDrCtle();
                Task.Delay(50);
            }
        }

        private void OutputSwingSliderTx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveCdrSwing();
        }

        private void OutputSwingSliderTx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveCdrSwing();
                Task.Delay(50);
            }
        }

        private void SaveCdrSwing()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
            ViewModel.UtbMonitoringPause();

            ViewModel.WriteCdrSwing((bool)cbMuteForce.IsChecked, (bool)cbDisAutoMute.IsChecked, (byte)OutputSwingSliderTx.Value, _channel, 1);

            ViewModel.UtbMonitoringRelease();
            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }

        }

        private void DeemphasisSliderTx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveDeephasis();
        }

        private void SaveDeephasis()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен
            ViewModel.UtbMonitoringPause();

            ViewModel.WriteCdrDeemphasis((bool)cbJAdj.IsChecked, (byte)DeemphasisSliderTx.Value, _channel, 1);

            ViewModel.UtbMonitoringRelease();
            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }

        }

        private void DeemphasisSliderTx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {               
                SaveDeephasis();
            }
        }

        private void CtleSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessTx = false;
        }

        private void OutputSwingSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessTx = false;
        }

        private void DeemphasisSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.ZeroChannel.IsOperationCdrSuccessTx = false;
        }
    }
}
