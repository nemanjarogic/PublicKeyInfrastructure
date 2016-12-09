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
        X509Certificate2 RegisterClient(string address);
    }
}
