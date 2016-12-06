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
    public class RegistrationAuthorityService : ChannelFactory<IRegistrationAuthorityContract>, IRegistrationAuthorityContract, IDisposable
    {
        private IRegistrationAuthorityContract factory;

        public RegistrationAuthorityService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public X509Certificate2 RegisterClient(string subjectName)
        {
            X509Certificate2 certificate = null;

            if(!String.IsNullOrEmpty(subjectName))
            {

            }

            return certificate;
        }
    }
}
