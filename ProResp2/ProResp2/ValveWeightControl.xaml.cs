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

namespace ProResp2
{
    /// <summary>
    /// Interaction logic for ValveWeightControl.xaml
    /// </summary>
    using System.Text.RegularExpressions;
    public partial class ValveWeightControl : UserControl
    {
        public ValveWeightControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public int ValveNum { get; set; }
        public string Weight { get; set; }

        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]"); // \.
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}