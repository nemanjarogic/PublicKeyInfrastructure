using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using Common.Client;
using System.Security.Principal;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client node\n\n");

            using(ServiceHost host = new ServiceHost(typeof(ClientService)))
            {
                Console.Write("Host service port: ");
                string port = Console.ReadLine();
                string address = string.Format("net.tcp://localhost:{0}/Client", port);
                host.AddServiceEndpoint(typeof(IClientContract), new NetTcpBinding(), address);
                    
                host.Open();
                Console.WriteLine("Service is started...");


                ClientProxy proxy = new ClientProxy(
                new EndpointAddress(string.Format("net.tcp://localhost:{0}/Client", port)),
                new NetTcpBinding(), new ClientService());

                while(true)
                {
                    Console.WriteLine("1.Connect to other client");
                    Console.WriteLine("2.Send message");
                    Console.WriteLine("3.End");

                    string option = Console.ReadLine();
                    if (option.Equals("3")) break;

                    switch(option)
                    {
                        case "1":
                            Console.WriteLine("Client address:");
                            string clientAddress = Console.ReadLine();

                            break;
                        case "2":
                            Console.WriteLine("Message: ");
                            Console.WriteLine("Address: ");
                            break;

                    }
                }

                Console.ReadKey();
            }
        }
    }
}
