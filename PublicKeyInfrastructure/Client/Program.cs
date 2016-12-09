using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using Common.Client;
using System.Security.Principal;
using Cryptography.AES;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client node\n\n");

            Console.Write("Host service port: ");
            string port = Console.ReadLine();
            string address = string.Format("net.tcp://localhost:{0}/Client", port);

            ServiceHost host = new ServiceHost(new ClientService(address));
            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = new TimeSpan(0, 0, 5);
            binding.ReceiveTimeout = new TimeSpan(0, 0, 5);
            binding.OpenTimeout = new TimeSpan(0, 0, 5);
            binding.CloseTimeout = new TimeSpan(0, 0, 5);

            host.AddServiceEndpoint(typeof(IClientContract), binding, address);
                    
            host.Open();
            Console.WriteLine("Service is started...");


            ClientProxy proxy = new ClientProxy(
            new EndpointAddress(string.Format("net.tcp://localhost:{0}/Client", port)),
            new NetTcpBinding(), new ClientService());

            while(true)
            {
                Console.WriteLine("\n1.Connect to other client");
                Console.WriteLine("2.Send message");
                Console.WriteLine("3.End");

                string option = Console.ReadLine();
                if (option.Equals("3")) break;

                switch(option)
                {
                    case "1":
                        Console.WriteLine("Client port:");
                        string clientAddress = Console.ReadLine();
                        proxy.StartComunication(clientAddress);

                        break;

                    case "2":
                        Console.WriteLine("Address: ");
                        string clientAddres = Console.ReadLine();
                        Console.WriteLine("Message: ");
                        string message = Console.ReadLine();

                        proxy.CallPay(System.Text.Encoding.UTF8.GetBytes(message), clientAddres);

                        break;

                }
            }

            Console.ReadKey();
            host.Close();
        }
    }
}
