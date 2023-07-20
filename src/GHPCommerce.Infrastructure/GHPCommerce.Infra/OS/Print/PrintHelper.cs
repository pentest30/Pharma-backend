using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Infra.OS.Print
{
    public class PrintHelper
    {
        private readonly IPAddress PrinterIPAddress;

        private readonly byte[] FileData;

        private readonly int PortNumber;
        private ManualResetEvent connectDoneEvent { get; set; }

        private ManualResetEvent sendDoneEvent { get; set; }

        public PrintHelper(byte[] fileData, string printerIPAddress, int portNumber = 9100)
        {
            FileData = fileData;
            PortNumber = portNumber;
            if (!IPAddress.TryParse(printerIPAddress, out PrinterIPAddress))
                throw new Exception("Wrong IP Address!");
        }

        public PrintHelper(byte[] fileData, IPAddress printerIPAddress, int portNumber = 9100)
        {
            FileData = fileData;
            PortNumber = portNumber;
            PrinterIPAddress = printerIPAddress;
        }
        private static int TotalPages = 0;
        private static int PrinterBufferSize =16;//pages
        private static bool locked = false;
        /// <inheritDoc />
        public async Task<bool> PrintData()
        {
            bool done = false;
            int i = 1;
            await LockProvider<string>.WaitAsync(PrinterIPAddress.ToString());
            while (!done && i<3)
            {
                i++;
                int pageCount = 2;
                //this line is Optional for checking before send data
                if (!NetworkHelper.CheckIPAddressAndPortNumber(PrinterIPAddress, PortNumber))
                {
                    LockProvider<string>.Release(PrinterIPAddress.ToString());
                    return false;
                }
                IPEndPoint remoteEP = new IPEndPoint(PrinterIPAddress, PortNumber);
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                    ProtocolType.Tcp);
                client.NoDelay = true;
                connectDoneEvent = new ManualResetEvent(false);
                sendDoneEvent = new ManualResetEvent(false);

                try
                {
                    
                    
                    TotalPages += pageCount;
                    if (TotalPages > PrinterBufferSize)
                    {
                        TotalPages = 0;
                        Thread.Sleep(1000 * 5);
                    }
                    client.BeginConnect(remoteEP, connectCallback, client);
                    connectDoneEvent.WaitOne();
                    var result = client.BeginSend(FileData, 0, FileData.Length, 0, sendCallback, client)
                        .AsyncWaitHandle.WaitOne(1 * 1000);
                    sendDoneEvent.WaitOne();
                    this.shutDownClient(client);
                    TotalPages -= pageCount;
                    done = true;                    
                }
                catch (Exception ex)
                {

                    this.shutDownClient(client);
                }
                Thread.Sleep(100 * 2);
            }
            LockProvider<string>.Release(PrinterIPAddress.ToString());



            return done;
        }
        public bool PrintFile(string filePath)
        {
            //this line is Optional for checking before send data
            if (!NetworkHelper.CheckIPAddressAndPortNumber(PrinterIPAddress, PortNumber))
                return false;
            IPEndPoint remoteEP = new IPEndPoint(PrinterIPAddress, PortNumber);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.NoDelay = true;
            connectDoneEvent = new ManualResetEvent(false);
            sendDoneEvent = new ManualResetEvent(false);

            try
            {
                client.BeginConnect(remoteEP, connectCallback, client);
                connectDoneEvent.WaitOne();
                client.Send(File.ReadAllBytes(filePath));
                //sendDoneEvent.WaitOne();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                // Shutdown the client
                this.shutDownClient(client);
            }
        }

        private void connectCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);

            // Signal that the connection has been made.
            connectDoneEvent.Set();
        }

        private void sendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);

            // Signal that all bytes have been sent.
            sendDoneEvent.Set();
        }
        private void shutDownClient(Socket client)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
