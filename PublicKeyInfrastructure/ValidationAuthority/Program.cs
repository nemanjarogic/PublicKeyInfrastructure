using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ValidationAuthority
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string address = "net.tcp://localhost:9999/ValidationAuthorityService";
            ServiceHost host = new ServiceHost(typeof(ValidationAuthorityService));
            host.AddServiceEndpoint(typeof(IValidationAuthorityContract), binding, address);

            try
            {
                host.Open();
                Console.WriteLine("ValidationAuthorityService is started.\nPress <enter> to stop ...");



                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] {0}", e.Message);
                Console.WriteLine("[StackTrace] {0}", e.StackTrace);
            }
            finally
            {
                host.Close();
            }
        }
    }
}
