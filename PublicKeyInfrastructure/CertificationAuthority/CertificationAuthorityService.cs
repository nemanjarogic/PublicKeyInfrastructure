using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
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

        }
    }
}
