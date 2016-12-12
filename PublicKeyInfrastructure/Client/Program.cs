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
using Common.Proxy;
using Client.Database;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.CancelKeyPress += CurrentDomain_ProcessExit;

            Console.WriteLine("Client node\n\n");
            Console.Write("Host service port: ");
            string port = Console.ReadLine();

            var localHost = Dns.GetHostEntry(Dns.GetHostName());
            string localIp = null;
            foreach (var ip in localHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip.ToString();
                    break;
                }
            }

            if(localIp == null)
            {
                Console.WriteLine("Faield to start client");
                Console.ReadLine();
                return;

            }
            string address = string.Format("net.tcp://{0}:{1}/Client", localIp, port);
            IDatabaseWrapper dbWrapper = new SQLiteWrapper();

            ServiceHost host = null;
            try
            {
                host = new ServiceHost(new ClientService(address, dbWrapper));
                NetTcpBinding binding = new NetTcpBinding();
                binding.SendTimeout = new TimeSpan(0, 5, 5);
                binding.ReceiveTimeout = new TimeSpan(0, 5, 5);
                binding.OpenTimeout = new TimeSpan(0, 5, 5);
                binding.CloseTimeout = new TimeSpan(0, 5, 5);

                host.AddServiceEndpoint(typeof(IClientContract), binding, address);

                host.Open();
                Console.WriteLine("Service is started...");
            }
            catch(Exception)
            {
                Console.WriteLine("Faield to start client");
                Console.ReadLine();
                return;
            }

            ClientProxy proxy = new ClientProxy(
                new EndpointAddress(string.Format("net.tcp://{0}:{1}/Client", localIp, port)),
                new NetTcpBinding(), new ClientService()
            );

            while(true)
            {
                Console.WriteLine("\n1.Connect to other client");
                Console.WriteLine("2.Send message");
                Console.WriteLine("3.Show database...");
                Console.WriteLine("4.End");

                string option = Console.ReadLine();
                if (option.Equals("4")) break;

                switch(option)
                {
                    case "1":
                        Console.Write("IP address:");
                        string ip = Console.ReadLine();
                        Console.WriteLine();
                        Console.Write("Port: ");
                        string clientPort = Console.ReadLine();
                        proxy.StartComunication(string.Format("net.tcp://{0}:{1}/Client", ip, clientPort));

                        break;

                    case "2":
                        Dictionary<int, string> clients = proxy.GetClients();

                        Console.WriteLine("===============================");
                        foreach(var c in clients)
                        {
                            Console.WriteLine("{0}.{1}", c.Key, c.Value);
                        }
                        Console.WriteLine("===============================");

                        Console.Write("Client number: ");
                        string clientNumString = Console.ReadLine();
                        Int32 clientNum;
                        if(Int32.TryParse(clientNumString, out clientNum))
                        {
                            string clientAddr = null;

                            if (clients.TryGetValue(clientNum, out clientAddr))
                            {
                                Console.Write("Message: ");
                                string message = Console.ReadLine();
                                try
                                {
                                    proxy.CallPay(System.Text.Encoding.UTF8.GetBytes(message), clientAddr);
                                    Console.WriteLine("Message is sent successfully");
                                }
                                catch(Exception)
                                {
                                    Console.WriteLine("Error while sending message. Try again.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Number is invalid");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Number is invalid");
                        }
                        
                        break;
                    case "3":
                        Console.WriteLine("Connected Clients:");
                        dbWrapper.ListAllRecordsFromTable();
                        break;
                }
            }

            Console.ReadKey();
            host.Close();
            
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            string address = "net.tcp://localhost:10002/RegistrationAuthorityService";
            using (RAProxy raProxy = new RAProxy(address, binding))
            {
                //caProxy.RemoveMeFromList();
            }
        }
    }
}
