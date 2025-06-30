using QsfpEyeDiagram.ViewModels;
using Std.Modules.ConfigurationParameters.Qsfp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QsfpEyeDiagram.Controls.Base
{
    public class QsfpEyeDiagramControlBase : UserControl
    {

        private QsfpEyeDiagramViewModel _viewModel;
        protected QsfpEyeDiagramViewModel ViewModel => _viewModel ?? (_viewModel = (QsfpEyeDiagramViewModel)DataContext);

        protected Style st = new Style();

        public QsfpEyeDiagramControlBase()
        {
            //Brush resBrush = Resources["NoActivePanelBrush"] as Brush;
            st.TargetType = typeof(QsfpEyeDiagramControlBase);
            //Setter Disable = new Setter() { Property = IsEnabledProperty, Value = false };
            //Setter Active = new Setter() { Property = IsEnabledProperty, Value = true };
            Setter ActiveBackGroundSetter = new Setter() { Property = BackgroundProperty, Value = Brushes.White };
            Setter InactiveBackgroundSetter = new Setter() { Property = BackgroundProperty, Value = Brushes.Gray };//Цвет фона неактивной панели
            Trigger MouseOver = new Trigger() { Property = IsMouseOverProperty, Value = true };
            MouseOver.Setters.Add(ActiveBackGroundSetter);
            //MouseOver.Setters.Add(Active);

            st.Setters.Add(InactiveBackgroundSetter);

            //st.Setters.Add(Disable);
            st.Triggers.Add(MouseOver);
            this.Style = st;
        }

    }
}
