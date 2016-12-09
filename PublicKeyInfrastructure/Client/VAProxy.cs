using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class VAProxy : ChannelFactory<IValidationAuthorityContract>, IValidationAuthorityContract,IDisposable
    {
        private IValidationAuthorityContract proxy; 

        public VAProxy(string address, NetTcpBinding binding)
            : base(binding, address)
        {
            proxy = this.CreateChannel();
        }

        public bool isCertificateValidate(X509Certificate2 certificate)
        {
            return proxy.isCertificateValidate(certificate);
        }
    }
}
