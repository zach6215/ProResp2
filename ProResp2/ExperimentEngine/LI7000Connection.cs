namespace ExperimentEngine
{
    using System;
    using LibUsbDotNet;
    using LibUsbDotNet.Main;
    using System.Text;

    internal class LI7000Connection
    {
        private UsbEndpointReader messageReader;
        private UsbEndpointReader dataReader;
        private UsbEndpointWriter writer;
        private UsbDevice LI7000;
        private UsbDeviceFinder LI7000Finder;
        private int readTimeLimit = 1000;
        private int writeTimeLimit = 1000;
        internal string DataHeader
        {
            get;
            private set;
        }

        public LI7000Connection()
        {
            this.LI7000Finder = new UsbDeviceFinder(0x1509);
            this.LI7000 = UsbDevice.OpenUsbDevice(LI7000Finder);

            IUsbDevice wholeLI7000 = LI7000 as IUsbDevice;

            //Setup interface if necessary
            if (!ReferenceEquals(wholeLI7000, null))
            {
                wholeLI7000.SetConfiguration(1);
                wholeLI7000.ClaimInterface(0);
            }

            if (LI7000 == null)
            {
                throw new Exception("LI7000 not found!");
            }

            //Open readers and writer
            this.messageReader = LI7000.OpenEndpointReader(ReadEndpointID.Ep01);
            this.dataReader = LI7000.OpenEndpointReader(ReadEndpointID.Ep06);
            this.writer = LI7000.OpenEndpointWriter(WriteEndpointID.Ep02);

            this.SetupLI7000();
        }

        private void SetupLI7000()
        {
            ErrorCode errorCode = ErrorCode.None;
            string configMessage = "(USB(Rate Polled)(Timestamp None)(Sources(\"CO2B um/m\" \"H2OB mm/m\" \"T C\")))"; // 
            int bytesWritten;
            string? response;

            this.ClearBuffers();

            errorCode = this.writer.Write(Encoding.Default.GetBytes(configMessage), writeTimeLimit, out bytesWritten);

            response = this.GetResponse(this.messageReader);

            if (response != "\nOK\n")
            {
                throw new Exception("Invalid LI7000 configuration! LI7000 responded with: " + response);
            }

            response = this.Poll();

            if (response != null && response.Substring(0, 5) == "DATAH")
            {
                response = response.Substring(7);
                response = response.Replace("B", string.Empty);
                response = response.Replace("\"", string.Empty);
                response = response.Replace("\n", string.Empty);
                response = response.Replace("um/m", "ppm");
                this.DataHeader = response;
            }
            else
            {
                throw new Exception("Internal Error: Invalid LI7000 Data Header!");
            }
        }

        private string? GetResponse(UsbEndpointReader argReader)
        {
            string? response = null;
            ErrorCode errorCode = ErrorCode.None;
            int bytesRead;

            do
            {
                byte[] readBuffer = new byte[1024];
                errorCode = argReader.Read(readBuffer, readTimeLimit, out bytesRead);

                if (bytesRead > 0)
                {
                    response += Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                }

            } while (bytesRead > 0);

            return response;
        }

        /// <summary>
        /// Clears both messageReader and dataReader
        /// </summary>
        /// <returns></returns>
        private string? ClearBuffers()
        {
            int bytesWritten;
            int bytesRead;
            ErrorCode errorCode = ErrorCode.None;
            string response = null;

            this.writer.Write(Encoding.Default.GetBytes(")"), 1000, out bytesWritten);

            do
            {
                byte[] readBuffer = new byte[1024];
                errorCode = this.messageReader.Read(readBuffer, 1000, out bytesRead);

                if (bytesRead > 0)
                {
                    response += Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                }

            } while (bytesRead > 0);

            do
            {
                byte[] readBuffer = new byte[1024];
                errorCode = this.dataReader.Read(readBuffer, 1000, out bytesRead);

                if (bytesRead > 0)
                {
                    response += Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                }
            } while (bytesRead > 0);

            return response;
        }

        public string? Poll()
        {
            string? responseMessage = null;
            string? responseData = null;
            ErrorCode errorCode = ErrorCode.None;
            int bytesWritten;

            //Error: System.ObjectDispoedExeption: 'Safe handle has been closed.'
            errorCode = this.writer.Write(Encoding.Default.GetBytes("(USB(Poll Now))"), this.writeTimeLimit, out bytesWritten);

            responseMessage = this.GetResponse(this.messageReader);

            if (responseMessage == "\nOK\n")
            {
                responseData = this.GetResponse(this.dataReader);
            }


            return responseData;
        }

        internal void CloseConnection()
        {
            if (LI7000 != null)
            {
                if (LI7000.IsOpen)
                {
                    IUsbDevice wholeLI7000 = LI7000 as IUsbDevice;
                    if (!ReferenceEquals(wholeLI7000, null))
                    {
                        wholeLI7000.ReleaseInterface(0);
                    }

                    LI7000.Close();
                }
            }
            LI7000 = null;
            UsbDevice.Exit();
        }
    }
}