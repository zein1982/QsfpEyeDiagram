using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
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

namespace QsfpEyeDiagram.Controls.CdrConfiguration
{
    /// <summary>
    /// Логика взаимодействия для FirstCdrTxControl.xaml
    /// </summary>
    public partial class FirstCdrTxControl : QsfpEyeDiagramControlBase
    {
        public readonly Channel _channel;
        public FirstCdrTxControl()
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
                Task.Delay(50).GetAwaiter().GetResult();
            }
        }

        private void SaveSwing()
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

        private void cbDisAutoMute_Click(object sender, RoutedEventArgs e)
        {
            SaveSwing();
        }

        private void cbMuteForce_Click(object sender, RoutedEventArgs e)
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

        private void cbJAdj_Click(object sender, RoutedEventArgs e)
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
                SaveDeemphasis();
            }

        }

        private void CtleSliderTx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.FirstChannel.IsOperationCdrSuccessTx = false;
        }

        private void OutputSwingSliderTx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.FirstChannel.IsOperationCdrSuccessTx = false;
        }

        private void DeemphasisSliderTx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.FirstChannel.IsOperationCdrSuccessTx = false;
        }
    }
}
