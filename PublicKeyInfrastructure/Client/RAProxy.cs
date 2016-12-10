using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Common.Proxy;

namespace Client
{
    public class RAProxy : ChannelFactory<IRegistrationAuthorityContract>, IRegistrationAuthorityContract, IDisposable
    {
        private IRegistrationAuthorityContract raProxy; 

        public RAProxy(string address, NetTcpBinding binding)
            : base(binding, address)
        {
            raProxy = this.CreateChannel();
        }

        public CertificateDto RegisterClient(string subjectName)
        {
            return raProxy.RegisterClient(subjectName);
        }
    }
}
