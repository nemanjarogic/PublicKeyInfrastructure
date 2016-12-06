using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationAuthority
{
    public class RegistrationAuthorityService : ChannelFactory<IRegistrationAuthorityContract>, IRegistrationAuthorityContract, IDisposable
    {
        private IRegistrationAuthorityContract factory;

        public RegistrationAuthorityService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {

        }
    }
}
