using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Proxy;
using Client;
using System.ServiceModel;

namespace TestHack
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Ip: ");
            string ip = Console.ReadLine();

            Console.Write("Port: ");
            string port = Console.ReadLine();
            ClientProxy proxy = new ClientProxy(new EndpointAddress(string.Format("net.tcp://{0}:{1}/Client", ip, port)),
                new NetTcpBinding(), new ClientService());

            Console.WriteLine("Success: {0}", proxy.Pay(new byte[100]));


            Console.ReadLine();
        }
    }
}
