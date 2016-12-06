using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ValidationAuthority
{
    public class ValidationAuthorityService : ChannelFactory<IValidationAuthorityContract>, IValidationAuthorityContract, IDisposable
    {
        private IValidationAuthorityContract factory;

        public ValidationAuthorityService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {

        }
    }
}
