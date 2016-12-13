using Common.Proxy;
using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationAuthority
{
    class Program
    {
        static void Main(string[] args)
        {
            //testCAProxy();
            /*Console.ReadLine();
            return;*/
            

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string address = "net.tcp://10.1.212.108:10002/RegistrationAuthorityService";
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
            CertificateDto certDto = null;
            Console.WriteLine("Test of using CAProxy in RA started...");
            RegistrationAuthorityService service = new RegistrationAuthorityService();
            certDto = service.RegisterClient("testClient");
            Console.WriteLine("Test of using CAProxy in RA finished. Name of new certificate - " + ((certDto.GetCert() != null) ? certDto.GetCert().SubjectName.ToString() : "registration not implemented"));
        }

        #endregion
    }
}
