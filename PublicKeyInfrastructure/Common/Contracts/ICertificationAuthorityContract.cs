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
    public interface ICertificationAuthorityContract
    {
        [OperationContract]
        X509Certificate2 GenerateCertificate(string subjectName);

        [OperationContract]
        bool WithdrawCertificate(X509Certificate2 certificate);
    }
}
