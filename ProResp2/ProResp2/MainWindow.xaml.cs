namespace ProResp2
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Microsoft.Win32;
    using System.Timers;
    using ExperimentEngine;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExperimentViewModel experimentViewModel = new ExperimentViewModel();
        List<CheckBox> valveCheckBoxes = new List<CheckBox>();

        public MainWindow()
        {
            InitializeComponent();

            this.experimentGroupBox.Visibility = Visibility.Collapsed;
            for (int i = 0; i < ExperimentViewModel.NumValves; i++)
            {
                CheckBox newCheckBox = new CheckBox();
                newCheckBox.Content = "Valve " + (i +1).ToString();
                this.valveCheckBoxes.Add(newCheckBox);
            }
            this.SetValveWeightGrid(this.valveWeightGrid);
            this.selectValveListBox.ItemsSource = this.valveCheckBoxes;
            this.stopButton.IsEnabled = false;
        }

        private void SetValveWeightGrid(Grid argGrid)
        {
            int numCols = 3;
            int numRows = 8;

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

            int valveNum = 1;
            for (int i = 0; i < numCols; i++)
            {
                for (int j = 0; j < numRows; j++)
                {
                    ValveWeightControl valveWeightControl = new ValveWeightControl();
                    valveWeightControl.ValveNum = valveNum;

                    Grid.SetColumn(valveWeightControl, i);
                    Grid.SetRow(valveWeightControl, j);
                    
                    argGrid.Children.Add(valveWeightControl);
                    valveNum++;
                }
            }
        }

        private void checkAllValvesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (experimentViewModel.ExperimentRunning == true)
            {
                MessageBox.Show("There is already an experiment running! Please stop this experiment before starting another.");
                return;
            }
            if (this.experimentViewModel.FilePath == null)
            {
                MessageBox.Show("No file selected! Please select a file to store data.");
                return;
            }

            List<int> valveNums = new List<int>();
            foreach (CheckBox item in this.valveCheckBoxes)
            {
                if (item.IsChecked == true)
                {
                    valveNums.Add(int.Parse(item.Content.ToString().Replace("Valve ", string.Empty)));
                }
            }

            if (valveNums.Count == 0)
            {
                MessageBox.Show("No valves selected! Please select at least one valve.");
                return;
            }

            try
            {
                experimentViewModel.StartNewExperiment(valveNums);

                //Active valve binding
                Binding binding = new Binding("ValveNum");
                binding.Source = this.experimentViewModel.ActiveValve;
                binding.StringFormat = "Active Valve: {0}";
                this.activeValveTextBlock.SetBinding(TextBlock.TextProperty, binding);

                //Current CO2 binding
                binding = new Binding("CO2");
                binding.Source = this.experimentViewModel.ActiveValve;
                binding.StringFormat = "Current CO2: {0}" + experimentViewModel.ActiveValve.CO2Units;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                this.currentCO2TextBlock.SetBinding(TextBlock.TextProperty, binding);

                //Current H20 binding
                binding = new Binding("H2O");
                binding.Source = this.experimentViewModel.ActiveValve;
                binding.StringFormat = "Current H2O: {0}" + experimentViewModel.ActiveValve.H2OUnits;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                this.currentH2OTextBlock.SetBinding(TextBlock.TextProperty, binding);

                //Current Temperature binding
                binding = new Binding("Temperature");
                binding.Source = this.experimentViewModel.ActiveValve;
                binding.StringFormat = "Current Temperature: {0}" + experimentViewModel.ActiveValve.TemperatureUnits;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                this.currentTempTextBlock.SetBinding(TextBlock.TextProperty, binding);

                //Current Flow binding
                binding = new Binding("Flow");
                binding.Source = this.experimentViewModel.ActiveValve;
                binding.StringFormat = "Current Flow: {0}" + experimentViewModel.ActiveValve.FlowUnits;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                this.currentFlowTextBlock.SetBinding(TextBlock.TextProperty, binding);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return;
            }

            //Change UI View
            this.NewExperiment_UpdateUI();
            //experimentEngine.Start();
        }

        private void NewExperiment_UpdateUI()
        {
            this.experimentGroupBox.Visibility = Visibility.Visible;
            this.valveWeightGroupBox.Visibility = Visibility.Collapsed;
            this.startButton.IsEnabled = false;
            this.checkAllValvesButton.IsEnabled = false;
            this.selectAllValvesButton.IsEnabled = false;
            this.stopButton.IsEnabled = true;
            this.createDataFileButton.IsEnabled = false;
            foreach (CheckBox item in this.valveCheckBoxes)
            {
                item.IsEnabled = false;
            }
        }

        private void createDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt";

            Nullable<bool> result = saveFileDialog1.ShowDialog();

            if (result == true)
            {
                experimentViewModel.FilePath = saveFileDialog1.FileName;
                this.dataFileTextBlock.Text = "Current Data File: " + experimentViewModel.FilePath;
            }
        }

        private void selectAllValvesButton_Click(object sender, RoutedEventArgs e)
        {
            foreach ( CheckBox item in this.valveCheckBoxes)
            {
                item.IsChecked = true;
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            experimentViewModel.StopExperiment();
            //CHANGE TO DATA BINDING
            this.dataFileTextBlock.Text = "Current Data File:";

            this.experimentGroupBox.Visibility = Visibility.Collapsed;
            this.valveWeightGroupBox.Visibility = Visibility.Visible;
            this.startButton.IsEnabled = true;
            this.checkAllValvesButton.IsEnabled = true;
            this.selectAllValvesButton.IsEnabled = true;
            this.createDataFileButton.IsEnabled = true;
            this.stopButton.IsEnabled = false;
            foreach (CheckBox item in this.valveCheckBoxes)
            {
                item.IsEnabled = true;
            }
        }
    }
}
