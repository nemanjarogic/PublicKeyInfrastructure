using Common.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
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
        CertificateDto GenerateCertificate(string subject, string address);

        [OperationContract]
        bool IsCertificateActive(X509Certificate2 certificate);

        [OperationContract]
        string WithdrawCertificate(string subjectName);

        [OperationContract]
        bool SaveCertificateToBackupDisc(CertificateDto certDto);

        [OperationContract]
        CAModelDto GetModel();

        [OperationContract]
        bool SetModel(CAModelDto param);
    }
}
