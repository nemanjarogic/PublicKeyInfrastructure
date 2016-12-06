using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    public class CertificationAuthorityService : ChannelFactory<ICertificationAuthorityContract>, ICertificationAuthorityContract, IDisposable
    {
        private ICertificationAuthorityContract factory;

        public CertificationAuthorityService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public X509Certificate2 GenerateCertificate(string subjectName)
        {
            throw new NotImplementedException();
        }
    }
}
