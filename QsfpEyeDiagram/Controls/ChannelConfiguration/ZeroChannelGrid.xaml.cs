using QsfpEyeDiagram.Controls.Base;
using Std.Modules.ConfigurationParameters.Qsfp;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace QsfpEyeDiagram.Controls.ChannelConfiguration
{
    /// <summary>
    /// Логика взаимодействия для ZeroChannelGrid.xaml
    /// </summary>
    public partial class ZeroChannelGrid : QsfpEyeDiagramControlBase
    {
        private readonly Channel _channel;

        public ZeroChannelGrid()
        {
            InitializeComponent();
            _channel = (Channel)Resources["Channel"];
        }

        private void WriteModulationValue(object sender, MouseButtonEventArgs e)
        {
            ViewModel.WriteModulationCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
            ViewModel._model.Bertwave.DisplayAutoScale();
        }

        private void WriteEqualizerValue(object sender, MouseButtonEventArgs e)
        {
            ViewModel.WriteEqualizerCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
            ViewModel._model.Bertwave.DisplayAutoScale();
        }

        private void WriteCrossingValue(object sender, MouseButtonEventArgs e)
        {
            
            ViewModel.WriteCrossingCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
            ViewModel._model.Bertwave.DisplayAutoScale();
        }

        private void WriteBiasValue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ViewModel.WriteBiasCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
            ViewModel._model.Bertwave.DisplayAutoScale();
        }

        private void DirectBiasEnter_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BiasSlider.Value = Convert.ToDouble(DirectBiasEnter.Text);
                ViewModel.WriteBiasCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                //TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                //MoveFocus(request);
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }

        private void BiasSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //ViewModel.UtbMonitoringPause();           
            ViewModel.WriteBiasCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
            //ViewModel.UtbMonitoringRelease();
            //ViewModel.ZeroChannel
            ViewModel._model.Bertwave.DisplayAutoScale();
        }

        private void BiasSlider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                ViewModel.WriteBiasCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }

        private void DirectModulationEnter_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ModulationSlider.Value = Convert.ToDouble(DirectModulationEnter.Text);
                ViewModel.WriteModulationCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }

        private void ModulationSlider_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                ViewModel.WriteModulationCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }

        private void EqualizerSlider_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                ViewModel.WriteEqualizerCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }

        private void CrossingSlider_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                ViewModel.WriteCrossingCommand.Execute(new object[] { _channel, ViewModel.ZeroChannel });
                ViewModel._model.Bertwave.DisplayAutoScale();
            }
        }


        private void tbAtt0_LostFocus(object sender, RoutedEventArgs e)
        {
            double att;
            TextBox tb = sender as TextBox;
            string text = tb.Text.Replace(',','.');
            tb.Text = text;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out att))
            {
                Properties.Settings.Default.ch0Att = att;
                Properties.Settings.Default.Save();
            }
            else tb.Text = (Properties.Settings.Default.ch0Att).ToString();
            
        }

    }
}
