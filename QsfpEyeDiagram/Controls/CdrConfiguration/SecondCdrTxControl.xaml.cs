using System.Threading.Tasks;
using System.Windows.Input;
using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
using Std.Modules.ConfigurationParameters.Qsfp.Cdr;

namespace QsfpEyeDiagram.Controls.CdrConfiguration
{
    /// <summary>
    /// Логика взаимодействия для ZeroCdrTxControl.xaml
    /// </summary>
    public partial class SecondCdrTxControl : QsfpEyeDiagramControlBase
    {
        public readonly Channel _channel;
        public SecondCdrTxControl()
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

        private void SaveSwing()
        {
            bool refreshTecParamFlag = ViewModel.RefreshTecParameters;
            ViewModel.RefreshTecParameters = false;//т.к обмен с CDR происходит долго, запретим на время мониторинг TEC чтобы не вмешивался в обмен

            ViewModel.UtbMonitoringPause();
            ViewModel.WriteCdrSwing((bool)cbMuteForse.IsChecked, (bool)cbDisAutoMute.IsChecked, (byte)OutputSwingSliderTx.Value, _channel, 1);
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

        private void cbMuteForse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveSwing();
        }

        private void OutputSwingSliderTx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveSwing();
        }

        private void OutputSwingSliderTx_PreviewKeyUp(object sender, KeyEventArgs e)
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
            ViewModel.WriteCdrDeemphasis((bool)cbJAdj.IsChecked, (byte)DeemphasisSliderTx.Value, _channel, 1);
            ViewModel.UtbMonitoringRelease();

            if (refreshTecParamFlag)
            {
                ViewModel.RefreshTecParameters = true;
            }
        }

        private void cbJAdj_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveDeemphasis();
        }

        private void DeemphasisSliderTx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SaveDeemphasis();
        }

        private void DeemphasisSliderTx_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                SaveSwing();
                Task.Delay(50);
            }
        }

        private void CtleSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.SecondChannel.IsOperationCdrSuccessTx = false;
        }

        private void OutputSwingSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.SecondChannel.IsOperationCdrSuccessTx = false;
        }

        private void DeemphasisSliderTx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.SecondChannel.IsOperationCdrSuccessTx = false;
        }
    }
}
