using Common.Proxy;
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
    public class ValidationAuthorityService : IValidationAuthorityContract
    {
        #region Fields

        private NetTcpBinding binding;
        private string address;

        #endregion

        #region Constructor

        public ValidationAuthorityService()
	    {
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            address = "net.tcp://localhost:9999/CertificationAuthority";
	    }

        #endregion

        #region Public methods

        public bool isCertificateValidate(X509Certificate2 certificate)
        {
            bool retValue = false;

            if (certificate != null)
            {
                using (CAProxy caProxy = new CAProxy(binding, address))
                {
                    //logic for certificate validation
                    //------ check start and end date
                    //------ check if it is in activeCerts list in CA
                    //------ check if it is NOT in CLR list in CA
                }
            }

            return retValue;
        }

        #endregion
    }
}
