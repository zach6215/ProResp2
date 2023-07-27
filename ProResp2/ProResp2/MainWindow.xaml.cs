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
using Microsoft.Win32;

namespace ProResp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int NumValves = 24;
        List<CheckBox> valveCheckBoxes = new List<CheckBox>();
        string filePath;

        public MainWindow()
        {
            InitializeComponent();
            this.experimentGroupBox.Visibility = Visibility.Collapsed;
            for (int i = 0; i < NumValves; i++)
            {
                CheckBox newCheckBox = new CheckBox();
                newCheckBox.Content = "Valve " + (i +1).ToString();
                this.valveCheckBoxes.Add(newCheckBox);
            }
            this.SetValveWeightGrid(this.valveWeightGrid);
            this.selectValveListBox.ItemsSource = this.valveCheckBoxes;
        }

        private void SetValveWeightGrid(Grid argGrid)
        {
            int numCols = 3;
            int numRows = 8;

            argGrid.ShowGridLines = true;

            //Create columns


            for (int i= 0; i < numCols; i++)
            {
                ColumnDefinition newColumnDefinition = new ColumnDefinition();
                newColumnDefinition.Width = new GridLength(1,GridUnitType.Star);
                argGrid.ColumnDefinitions.Add(newColumnDefinition);
            }

            for (int i= 0; i < numRows; i++)
            {
                RowDefinition newRowDefinition = new RowDefinition();
                newRowDefinition.Height = new GridLength(1, GridUnitType.Star);
                argGrid.RowDefinitions.Add(newRowDefinition);
            }

            for (int i = 0; i < NumValves; i++)
            {

            }

        }

        private void checkAllValvesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void createDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt";

            Nullable<bool> result = saveFileDialog1.ShowDialog();

            if (result == true)
            {
                this.filePath = saveFileDialog1.FileName;
                this.dataFileTextBlock.Text = "Current Data File: " + this.filePath;
            }
        }
    }
}
