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

        public X509Certificate2 RegisterClient(string address)
        {
            X509Certificate2 certificate = null;

            if (!String.IsNullOrEmpty(address))
            {
                string subject = ServiceSecurityContext.Current.PrimaryIdentity.Name.Replace('\\','_').Trim();
                certificate = CAProxy.GenerateCertificate(subject, address);
            }

            return certificate;
        }

        #endregion
    }
}
