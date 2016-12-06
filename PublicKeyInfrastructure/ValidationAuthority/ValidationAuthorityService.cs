using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ValidationAuthority
{
    public class ValidationAuthorityService : ChannelFactory<IValidationAuthorityContract>, ValidationAuthority, IDisposable
    {
        private ValidationAuthority factory;

        public ValidationAuthorityService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {

        }
    }
}
