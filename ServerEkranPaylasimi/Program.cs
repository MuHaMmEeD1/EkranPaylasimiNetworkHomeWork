

using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;


namespace Pr
{
    class Program
    {

        static UdpClient Server { get; set; }
        static IPEndPoint EndPoint { get; set; }
        static List<IPEndPoint> ClientsEPs;
        static bool Isleyen = false;


        static void Main(string[] args)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27001);
            Server = new UdpClient(EndPoint);
            ClientsEPs = new List<IPEndPoint>();


            while (true)
            {
                IPEndPoint? remoutEP = null;
                var Bytes = Server.Receive(ref remoutEP);
                var len = Bytes.Length;
                if (len == 1)
                {
                    Console.WriteLine($"Yeni Qosulan {remoutEP.Port}");
                    ClientsEPs.Add(remoutEP);
                }
                else
                {
                    foreach (var Cl in ClientsEPs)
                    {
                        Console.WriteLine(Cl.Port);
                        Server.Send(Bytes, Cl);
                    }


                }

            }

        }

      

    }



}










