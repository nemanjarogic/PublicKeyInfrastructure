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
    [ServiceContract]
    public interface IClientContract
    {
        [OperationContract]
        bool InitiateComunication(X509Certificate othersideCertificate, NetTcpBinding binding, EndpointAddress address);
        [OperationContract]
        bool Pay(byte[] message);

        [OperationContract]
        void SetMessageKey(byte[,] messageKey);

        X509Certificate Register();
        byte[,] RandomGenerateKey();
    }
}
