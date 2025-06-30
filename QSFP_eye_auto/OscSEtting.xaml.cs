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
using System.Windows.Shapes;

namespace QSFP_eye_auto
{
    /// <summary>
    /// Логика взаимодействия для OscSEtting.xaml
    /// </summary>
    public partial class OscSEtting : Window
    {
        public OscSEtting()
        {
            InitializeComponent();
            tbOscIp.Text = Properties.Settings.Default.BertIP;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.BertIP = tbOscIp.Text;
            Properties.Settings.Default.Save();

        }
    }
}
