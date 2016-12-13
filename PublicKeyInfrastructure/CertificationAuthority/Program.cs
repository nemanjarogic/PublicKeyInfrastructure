using Common.Proxy;
using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    class Program
    {
        static Semaphore mainSemaphore = new Semaphore(2, 2, "mainSemaphore");
        
        static void Main(string[] args)
        {
            if (!mainSemaphore.WaitOne(0, false))
            {
                return;
            }
            Console.WriteLine("insert in main!!!");

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string address = "";

            if (mainSemaphore.WaitOne(0, false))
            {
                address = "net.tcp://localhost:10000/CertificationAuthority";
                mainSemaphore.Release();
            }
            else
            {
                address = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";
            }

            //string address = "net.tcp://localhost:10000/CertificationAuthority";
            //string address = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";

            ServiceHost host = null;

            int menuOption = 0;
            bool hostOpened = false;

            try
            {
        
                while (true)
                {
                    #region Menu print
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine("--MENU--");
                    if (!hostOpened)
                    {
                        Console.WriteLine("1. Turn server ON");
                    }
                    else
                    {
                        Console.WriteLine("1. Turn server OFF");
                    }
                    Console.WriteLine("2. Withdraw certificate...");
                    Console.WriteLine("------------------------------------------------------------");
                    Console.Write("Insert menu option: ");
                    menuOption = Int32.Parse(Console.ReadLine());
                    #endregion

                    if (menuOption == 1)
                    {
                        if (!hostOpened)
                        {
                            //open host
                            host = new ServiceHost(typeof(CertificationAuthorityService));
                            host.AddServiceEndpoint(typeof(ICertificationAuthorityContract), binding, address);
                            host.Open();
                            hostOpened = true;
                            Console.WriteLine("CertificationAuthority is started [address: {0}].\nPress <enter> to stop ...", address);
                        }
                        else
                        {
                            //close host
                            host.Abort();
                            host.Close();
                            hostOpened = false;
                            Console.WriteLine("Host closed [address: {0}]", address);
                        }
                    }
                    else if (menuOption == 2)
                    {
                        //withdrawing certificate using proxy
                        string certName = null;
                        bool succ = false;
                        Console.WriteLine("Insert certificate name:");
                        certName = Console.ReadLine();
                        succ = CAProxy.WithdrawCertificate(certName);
                        if (succ)
                        {
                            Console.WriteLine("Withdrawing successfull");
                        }
                        else
                        {
                            Console.WriteLine("Withdrawing not successfull");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                if (host != null)
                {

                    host.Abort();
                    host.Close();
                }
            }

            mainSemaphore.Release();
        }
    }
}
