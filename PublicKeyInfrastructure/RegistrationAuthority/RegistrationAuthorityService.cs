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

        public CertificateDto RegisterClient(string address)
        {
            CertificateDto certDto = null;

            if (!String.IsNullOrEmpty(address))
            {
                string subject = ServiceSecurityContext.Current.PrimaryIdentity.Name.Replace('\\','_').Trim();
                string port = address.Split(':')[2].Split('/')[0];
                subject = subject.Replace('-', '_') + port;
                certDto = CAProxy.GenerateCertificate(subject, address);
            }

            return certDto;
        }

        #endregion
    }
}
