using Common.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    public class RAProxy : ChannelFactory<IRegistrationAuthorityContract>, IRegistrationAuthorityContract, IDisposable
    {
        private IRegistrationAuthorityContract raProxy;

        public X509Certificate2 RegisterClient(string subjectName)
        {
            return raProxy.RegisterClient(subjectName);
        }
    }
}
