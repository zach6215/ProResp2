namespace ExperimentEngine
{
    using System;
    using System.Timers;
    using System.Collections.Generic;

    public class ExperimentEngine
    {
        private LinkedList<Valve> valvesList;
        private LinkedListNode<Valve> currentNode;
        private LI7000Connection LI7000;
        private string lI7000DataHeader;
        private string dateTimeHeader;
        private DateTime startDateTime;

        public string DateTimeHeader { get { return dateTimeHeader; } private set { dateTimeHeader = value; } }
        public string LI7000DataHeader { get { return lI7000DataHeader; } private set { lI7000DataHeader = value; } }
        public double DaysSinceStart { get { return (DateTime.Now.Date - startDateTime.Date).TotalDays; } }
        public Valve ActiveValve { get { return currentNode.Value; } }


        public ExperimentEngine(List<int> argActiveValves, List<double> argValveWeights)
        {
            this.valvesList = new LinkedList<Valve>();
            this.LI7000 = new LI7000Connection();

            this.LI7000DataHeader = this.LI7000.DataHeader;
            this.DateTimeHeader = "Date (mm/dd/yyyy) \t Time (hh:mm)";

            string[] units = LI7000DataHeader.Split('\t');

            int j = 0;
            foreach (int valveNum in argActiveValves)
            {
                Valve newValve = new Valve(valveNum, argValveWeights[j]);

                for (int i = 0; i < units.Length; i++)
                {
                    if (units[i].Contains("CO2"))
                    {
                        units[i] = units[i].Substring(units[i].IndexOf(' ') + 1);
                        newValve.CO2Units = units[i];
                    }
                    else if (units[i].Contains("H2O"))
                    {
                        units[i] = units[i].Substring(units[i].IndexOf(' ') + 1);
                        newValve.H2OUnits = units[i];
                    }
                    else if (units[i].Contains('T'))
                    {
                        units[i] = units[i].Substring(units[i].IndexOf(' ') + 1);
                        newValve.TemperatureUnits = units[i];
                    }
                }
                LinkedListNode<Valve> newNode = new LinkedListNode<Valve>(newValve);
                this.valvesList.AddLast(newNode);
            }

            this.currentNode = valvesList.First;
        }

        public void Poll()
        {
            this.currentNode.Value.MeasurementDateTime = DateTime.Now;
            string? response = LI7000.Poll();
            string newData = string.Empty;

            if (response != null && response.Substring(0, 5) == "DATA\t")
            {
                response = response.Substring(5);
                response = response.Replace("\n", string.Empty);

                string[] headers = LI7000DataHeader.Split('\t');
                string[] data = response.Split('\t');
                for (int i = 0; i < headers.Length; i++)
                {
                    switch (headers[i][0])
                    {
                        case 'C':
                            this.currentNode.Value.CO2 = double.Parse(data[i]);
                            break;
                        case 'H':
                            this.currentNode.Value.H2O = double.Parse(data[i]);
                            break;
                        case 'T':
                            this.currentNode.Value.Temperature = double.Parse(data[i]);
                            break;
                    }
                }
            }
        }

        public void ChangeValve()
        {
            if (this.currentNode.Next == null)
            {
                this.currentNode = this.valvesList.First;
            }
            else
            {
                this.currentNode = this.currentNode.Next;
            }
            this.Poll();
        }

        public void Stop()
        {
            //Disconnect all devices
            if (this.LI7000 != null)
            {
                this.LI7000.CloseConnection();
            }

            //Close stream
        }
    }
}