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
    public class RegistrationAuthorityService : IRegistrationAuthorityContract
    {
        #region Fields

        /*private NetTcpBinding binding;
        private string address;*/

        #endregion
        
        #region Constructor

        public RegistrationAuthorityService()
	    {
            /*binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            address = "net.tcp://localhost:9999/CertificationAuthority";*/
	    }

        #endregion

        #region Public methods

        public X509Certificate2 RegisterClient(string subjectName)
        {
            X509Certificate2 certificate = null;

            if(!String.IsNullOrEmpty(subjectName))
            {
                /*using(CAProxy caProxy = new CAProxy(binding, address))
                {
                    certificate = caProxy.GenerateCertificate(subjectName);
                }*/

                certificate = CAProxy.GenerateCertificate(subjectName);
            }

            return certificate;
        }

        #endregion
    }
}
