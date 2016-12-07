using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationAuthority
{
    class Program
    {
        static void Main(string[] args)
        {
            testCAProxy();
            return;
            

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string address = "net.tcp://localhost:9999/RegistrationAuthorityService";
            ServiceHost host = new ServiceHost(typeof(RegistrationAuthorityService));
            host.AddServiceEndpoint(typeof(IRegistrationAuthorityContract), binding, address);

            try
            {
                host.Open();
                Console.WriteLine("RegistrationAuthorityService is started.\nPress <enter> to stop ...");



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

        #region Test methods

        public static void testCAProxy()
        {
            RegistrationAuthorityService service = new RegistrationAuthorityService();
            service.RegisterClient("testClient");
        }

        #endregion
    }
}
