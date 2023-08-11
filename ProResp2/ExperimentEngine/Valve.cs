namespace ExperimentEngine
{
    using System.ComponentModel;
    public class Valve : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private double cO2;
        private double h2O;
        private double temperature;
        private double flow;


        public int ValveNum { get; private set; }
        public double Weight { get; private set; }
        public DateTime MeasurementDateTime { get; internal set; }

        public double CO2 
        { 
            get { return cO2; }
            internal set { cO2 = value; PropertyChanged.Invoke(this, new PropertyChangedEventArgs("CO2")); } 
        }
        public string CO2Units { get; internal set; }

        public double H2O 
        {
            get { return h2O; }
            internal set { h2O = value; PropertyChanged.Invoke(this, new PropertyChangedEventArgs("H2O")); } 
        }
        public string H2OUnits { get; internal set; }

        public double Temperature
        {
            get { return temperature; }
            internal set { temperature = value; PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Temperature")); }
        }
        public string TemperatureUnits { get; internal set; }

        public double Flow { get; internal set; }
        public string FlowUnits { get; internal set; }

        public Valve(int argValveNum)
        {
            this.ValveNum = argValveNum;
        }

        public Valve(int argValveNum, double argWeight)
        {
            this.ValveNum = argValveNum;
            this.Weight = argWeight;
        }

    }
}
