using Common.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Server
{
    [ServiceContract]
    public interface IRegistrationAuthorityContract
    {
        [OperationContract]
        CertificateDto RegisterClient(string address);

        [OperationContract]
        bool RemoveActiveClient();
    }
}
