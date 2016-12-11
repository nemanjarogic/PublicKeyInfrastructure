using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Common.Proxy;

namespace Common.Client
{
    [ServiceContract(CallbackContract = typeof(IClientContract), SessionMode = SessionMode.Required)]
    public interface IClientContract
    {
        [OperationContract]
        void CallPay(byte[] message, string address);

        [OperationContract]
        void Pay(byte[] message);

        [OperationContract]
        void StartComunication(string address);

        [OperationContract]
        String GetSessionId();
        
        [OperationContract]
        CertificateDto SendCert(CertificateDto cert);
        [OperationContract]
        bool SendKey(byte[] key);
        [OperationContract]
        object GetSessionInfo(string otherAddress);
    }
}
