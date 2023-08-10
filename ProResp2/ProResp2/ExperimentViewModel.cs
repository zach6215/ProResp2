using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProResp2
{
    using System.Windows.Threading;
    using ExperimentEngine;
    using System.IO;
    internal class ExperimentViewModel
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
        String[] valveWieghts = new String[24];

        public string? FilePath { get { return this.filePath; } set { this.filePath = value; } }
        public bool ExperimentRunning { get { return experimentRunning;} }
        public Valve ActiveValve { get { return this.activeValve; } set { this.activeValve = value; } }
        public String ValveSwitchMin { get { return this.valveSwitchMin; } set { this.valveSwitchMin = value;} }

        Valve activeValve;
        Valve previousValve;

        public ExperimentViewModel()
        {
            this.ValveSwitchMin = "15";
        }

        internal void StartNewExperiment(List<int> argValveNums)
        {

            this.experimentEngine = new ExperimentEngine(argValveNums);

            this.ActiveValve = experimentEngine.ActiveValve;

            this.pollDataTimer = new DispatcherTimer();
            this.pollDataTimer.Interval = TimeSpan.FromSeconds(this.dataPollSec);
            this.pollDataTimer.Tick += this.PollData;

            this.experimentRunning = true;
            this.WriteDataHeader();
            this.pollDataTimer.Start();
        }

        internal void StopExperiment()
        {
            this.pollDataTimer?.Stop();
            this.valveSwitchTimer?.Stop();
            this.experimentEngine?.Stop();
            this.experimentEngine = null;
            this.filePath = String.Empty;
            this.experimentRunning = false;
        }

        private void PollData(object sender, EventArgs e)
        {
            experimentEngine.Poll();
            this.WriteValveData(this.ActiveValve);
        }

        private void SwitchValves(object sender, EventArgs e)
        {

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
    }
}
