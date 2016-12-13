using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ValidationAuthority
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

            string address = "net.tcp://10.1.212.108:10003/ValidationAuthorityService";
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

        #region Test methods

        public static void testCAProxy()
        {
            bool isCertValidate = false;
            Console.WriteLine("Test of using CAProxy in VA started...");
            ValidationAuthorityService service = new ValidationAuthorityService();
            isCertValidate = service.isCertificateValidate(new X509Certificate2());
            Console.WriteLine("Test of using CAProxy in VA finished. Result of certValidation - " + isCertValidate);
        }

        #endregion
    }
}
