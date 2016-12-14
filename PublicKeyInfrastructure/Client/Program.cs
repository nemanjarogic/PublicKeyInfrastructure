using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using System.Security.Principal;
using Cryptography.AES;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Runtime.InteropServices;
using Client.Database;
using Common.Client;

namespace Client
{
    class Program
    {
        public static ClientService clientService;

        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        static void Main(string[] args)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            #region Starting service

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

            if (localIp == null)
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
                clientService = new ClientService(address, dbWrapper);
                host = new ServiceHost(clientService);
                NetTcpBinding binding = new NetTcpBinding();
                binding.SendTimeout = new TimeSpan(0, 5, 5);
                binding.ReceiveTimeout = new TimeSpan(0, 5, 5);
                binding.OpenTimeout = new TimeSpan(0, 5, 5);
                binding.CloseTimeout = new TimeSpan(0, 5, 5);
                host.AddServiceEndpoint(typeof(IClientContract), binding, address);

                host.Open();
                Console.WriteLine("Service is started...");
            }
            catch (Exception)
            {
                Console.WriteLine("Faield to start client");
                Console.ReadLine();
                return;
            }
            #endregion

            while (true)
            {
                Console.WriteLine("\n1.Connect to other client...");
                Console.WriteLine("2.Send message...");
                Console.WriteLine("3.Show database...");
                Console.WriteLine("4.Register...");
                Console.WriteLine("5.Test invalid certificate...");
                Console.WriteLine("6.End");

                string option = Console.ReadLine();
                if (option.Equals("6")) break;

                switch (option)
                {
                    #region Connect to client
                    case "1":
                        Console.Write("IP address:");
                        string ip = Console.ReadLine();
                        Console.WriteLine();
                        Console.Write("Port: ");
                        string clientPort = Console.ReadLine();
                        try
                        {
                            clientService.StartComunication(string.Format("net.tcp://{0}:{1}/Client", ip, clientPort));
                            Console.WriteLine("Session is opened");
                        }
                        catch(Exception)
                        {
                            Console.WriteLine("Failed to start communication");
                        }
                        break;
                    #endregion

                    #region Send message
                    case "2":
                        
                        Dictionary<int, string> clients = clientService.GetClients();
                        if(clients.Count == 0)
                        {
                            Console.WriteLine("Unable to send message. Connect to clients, and try again!");
                            break;
                        }

                        Console.WriteLine("===============================");
                        foreach (var c in clients)
                        {
                            Console.WriteLine("{0}.{1}", c.Key, c.Value);
                        }
                        Console.WriteLine("===============================");

                        Console.Write("Client number: ");
                        string clientNumString = Console.ReadLine();
                        Int32 clientNum;
                        if (Int32.TryParse(clientNumString, out clientNum))
                        {
                            string clientAddr = null;

                            if (clients.TryGetValue(clientNum, out clientAddr))
                            {
                                Console.Write("Message: ");
                                string message = Console.ReadLine();
                                try
                                {
                                    clientService.CallPay(System.Text.Encoding.UTF8.GetBytes(message), clientAddr);
                                    Console.WriteLine("Message is sent successfully");
                                }
                                catch (Exception)
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
                        #endregion

                    #region List clients
                    case "3":
                        Console.WriteLine("Connected Clients:");
                        dbWrapper.ListAllRecordsFromTable();
                        break;
                    #endregion

                    #region Register client
                    case "4":
                        clientService.LoadMyCertificate();
                        break;
                    #endregion

                    case "5":
                        clientService.TestInvalidCertificate();
                        break;

                }
            }

            ConsoleEventCallback(2);
            host.Close();
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (clientService!=null && (eventType == 2 || eventType == 0))
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                string address = "net.tcp://localhost:10002/RegistrationAuthorityService";
                var raProxy = new RAProxy(address, binding);
                using (new OperationContextScope(raProxy.GetChannel()))
                {
                    string myAddress = clientService.HostAddress;
                    clientService.RemoveInvalidClient(myAddress);
                    
                    MessageHeader aMessageHeader = MessageHeader.CreateHeader("UserName", "", clientService.ServiceName);
                    OperationContext.Current.OutgoingMessageHeaders.Add(aMessageHeader);
                    
                    //caProxy.RemoveMeFromList();
                    raProxy.RemoveActiveClient();
                }
                raProxy.Close();
            }
            return false;
        }

    }
}
