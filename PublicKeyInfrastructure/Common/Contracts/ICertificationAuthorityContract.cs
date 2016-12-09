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
        X509Certificate2 GenerateCertificate(string subject, string address);

        [OperationContract]
        bool IsCertificateActive(X509Certificate2 certificate);

        [OperationContract]
        bool WithdrawCertificate(X509Certificate2 certificate);

        [OperationContract]
        FileStream GetFileStreamOfCertificate(string certFileName);

        [OperationContract]
        bool SaveCertificateToBackupDisc(X509Certificate2 certificate, FileStream stream, string certFileName);

        [OperationContract]
        object GetModel();

        [OperationContract]
        bool SetModel(object param);
    }
}
