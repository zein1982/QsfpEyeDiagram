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
    }
}
