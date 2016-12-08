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
    [ServiceContract(CallbackContract = typeof(IClientContract))]
    public interface IClientContract
    {
        [OperationContract]
        void InitiateComunication(X509Certificate othersideCertificate);
        [OperationContract]
        void SetMessageKey(byte[] messageKey);
        [OperationContract]
        void Pay(byte[] message);

        [OperationContract]
        void StartComunication(string address);
        [OperationContract]
        void AcceptComunication(X509Certificate myCertificate);
        [OperationContract]
        void ReadyForMessaging();

        [OperationContract]
        String GetSessionId();
        
        X509Certificate Register();
        byte[] RandomGenerateKey();
        
    }
}
