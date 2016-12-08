using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            //string address = "net.tcp://localhost:10000/CertificationAuthority";
            string address = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";
            ServiceHost host = new ServiceHost(typeof(CertificationAuthorityService));
            host.AddServiceEndpoint(typeof(ICertificationAuthorityContract), binding, address);

            try
            {
                host.Open();
                Console.WriteLine("CertificationAuthority is started.\nPress <enter> to stop ...");
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
        }
    }
}
