using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MccDaq;

namespace ProResp2
{
    using System.Windows.Threading;
    using ExperimentEngine;
    using System.IO;
    using System.ComponentModel;
    internal class ExperimentViewModel : INotifyPropertyChanged
    {
        //private static bool experimentRunning = false;
        private ExperimentEngine? experimentEngine;
        bool experimentRunning;
        DispatcherTimer pollDataTimer;
        DispatcherTimer valveSwitchTimer;
        private string? filePath;
        public const int NumValves = 24;
        private int dataPollSec = 5;
        private String valveSwitchMin = "15";
        private DateTime startDateTime;
        private String[] valveWieghts = new String[24];
        public event PropertyChangedEventHandler? PropertyChanged;
        MccBoard board = new MccBoard(0);

        public string? FilePath { get { return this.filePath; } set { this.filePath = value; } }
        public bool ExperimentRunning { get { return experimentRunning;} }
        public Valve ActiveValve { get { return this.activeValve; } set { this.activeValve = value; } }
        public String ValveSwitchMin { get { return this.valveSwitchMin; } 
            set { this.valveSwitchMin = value; PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ValveSwitchMin")); } }

        Valve activeValve;
        Valve previousValve;

        

        public ExperimentViewModel()
        {
        }

        internal void StartNewExperiment(List<int> argValveNums, List<double> argValveWeights)
        {
            if (valveSwitchMin == null)
            {
                valveSwitchMin = "15";
            }
            if (!int.TryParse(valveSwitchMin, out int valveSwitchTime))
            {
                throw new Exception("Invalid valve switch time!");
            }

            this.experimentEngine = new ExperimentEngine(argValveNums, argValveWeights);

            this.ActiveValve = experimentEngine.ActiveValve;

            this.pollDataTimer = new DispatcherTimer();
            this.pollDataTimer.Interval = TimeSpan.FromSeconds(this.dataPollSec);
            this.pollDataTimer.Tick += this.PollData;

            this.experimentRunning = true;
            this.WriteDataHeader();
            this.pollDataTimer.Start();
            this.valveSwitchTimer.Start();
        }

        internal void StopExperiment()
        {
            this.pollDataTimer?.Stop();
            this.valveSwitchTimer?.Stop();
            this.experimentEngine?.Stop();
            this.experimentEngine = null;
            this.filePath = String.Empty;
            this.experimentRunning = false;
            this.TurnOffAllPorts();
        }

        private void PollData(object sender, EventArgs e)
        {
            experimentEngine.Poll();
            this.WriteValveData(this.ActiveValve);
        }

        public void SwitchValves(object sender, EventArgs e)
        {
            int currentValveNum = experimentEngine.ActiveValve.ValveNum;
            experimentEngine.ChangeValve();
            int nextValveNum = experimentEngine.ActiveValve.ValveNum;

            open(nextValveNum-1);
            close(currentValveNum-1);
            
        }

        private void WriteDataHeader()
        {
            string header = string.Empty;
            if (this.experimentEngine != null)
            {
                header = "Day of Experiment\t";
                header += this.experimentEngine.DateTimeHeader + "\t" + "Valve\t" + this.experimentEngine.LI7000DataHeader;
                header = header.Replace("pp/m", "(pp/m)").Replace("mm/m", "(mm/m)").Replace("T C", "Temp ('C)");

                using (StreamWriter sw = new StreamWriter(this.filePath))
                {
                    sw.WriteLine(header);
                    sw.Close();
                }
            }
        }

        private void WriteValveData(Valve argValve)
        {
            string valveData = string.Empty;

            valveData = this.experimentEngine.DaysSinceStart.ToString() + "\t";

            //Datetime
            valveData += argValve.MeasurementDateTime.ToString("MM/dd/yyyy") + "\t";
            valveData += argValve.MeasurementDateTime.ToString("H:mm") + "\t";

            //Data
            valveData += argValve.ValveNum + "\t";
            valveData += argValve.CO2.ToString() + "\t";
            valveData += argValve.H2O.ToString() + "\t";
            valveData += argValve.Temperature.ToString() + "\t";
            valveData += argValve.Flow.ToString() + "\t";

            using(StreamWriter sw = new StreamWriter(this.filePath, true))
            {
                sw.WriteLine(valveData);
                sw.Close();
            }
        }
        
        public void CheckAllPorts()
        {
            config();
            int numPorts = 24;

            for (int i = 0; i < numPorts; i++)
            {

                Console.WriteLine("Testing port " + i);

                SetPort(board, i, DigitalLogicState.High);
                Thread.Sleep(2000);

                SetPort(board, i, DigitalLogicState.Low);
            }
        }
        void config()
        {

            ConfigurePort(board, DigitalPortType.FirstPortA);
            ConfigurePort(board, DigitalPortType.FirstPortB);
            ConfigurePort(board, DigitalPortType.FirstPortCH);
            ConfigurePort(board, DigitalPortType.FirstPortCL);
        }
        void ConfigurePort(MccBoard board, DigitalPortType portType)
        {
            DigitalPortDirection direction = DigitalPortDirection.DigitalOut;
            board.DConfigPort(portType, direction);
        }

        void SetPort(MccBoard board, int portNum, DigitalLogicState state)
        {
            board.DBitOut(DigitalPortType.FirstPortA, portNum, state);
        }

        private void open(int current)
        {
            SetPort(board, current, DigitalLogicState.High);
        }
        private void close(int current)
        {
            SetPort(board, current, DigitalLogicState.Low);
        }
        private void TurnOffAllPorts()
        {
            config();
            // Turn off all ports
            for (int i = 0; i < 24; i++)
            {
                SetPort(board, i, DigitalLogicState.Low);
            }
        }
    }
}
