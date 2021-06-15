using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ToGeometryConverter.Object.UDP
{
    public class UdpLaserListener
    {
        public event EventHandler<byte[]> IncomingData;

        public bool Status { get; set; } = false;

        private UdpClient udpClient;
        private IPEndPoint groupEP;
        private int port;


        public UdpLaserListener(int port)
        {
            if (this.udpClient == null)
            {
                this.port = port;
            }
        }

        public async Task Run()
        {
            Status = true;

            while (Status)
            {
                this.udpClient = new UdpClient(port);
                this.groupEP = new IPEndPoint(IPAddress.Loopback.Address, port);
                try
                {
                    byte[] bytes = await Task<byte[]>.Factory.StartNew(() =>
                    {
                        Console.WriteLine("Waiting for broadcast");
                        return udpClient.Receive(ref groupEP);
                    });
                    IncomingData?.Invoke(this, bytes);
                }
                catch (SocketException er)
                {
                    Console.WriteLine(er);
                    Status = false;
                }
                finally
                {
                    udpClient.Close();
                }
            }
        }

        public async Task Stop()
        {
            udpClient.Close();
        } 
    }
}
