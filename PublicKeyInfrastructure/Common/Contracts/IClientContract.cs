using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

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
        X509Certificate2 SendCert(X509Certificate2 cert);
        [OperationContract]
        bool SendKey(byte[] key);
        [OperationContract]
        object GetSessionInfo(string otherAddress);
    }
}
