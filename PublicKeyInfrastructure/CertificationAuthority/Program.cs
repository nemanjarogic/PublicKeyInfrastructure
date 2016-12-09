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
            ServiceHost host = new ServiceHost(typeof(CertificationAuthorityService));
            host.AddServiceEndpoint(typeof(ICertificationAuthorityContract), binding, address);
            
            try
            {
                host.Open();
                Console.WriteLine("CertificationAuthority is started [address: {0}].\nPress <enter> to stop ...", address);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                host.Abort();
                host.Close();
            }

            mainSemaphore.Release();
        }
    }
}
