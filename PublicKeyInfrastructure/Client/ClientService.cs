using Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientService : ChannelFactory<IClientContract>, IClientContract, IDisposable
    {
        private IClientContract clientProxy;
        private VAProxy vaProxy;
        private RAProxy raProxy;
        private byte[,] messageKey;

        public ClientService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            vaProxy = new VAProxy();
            raProxy = new RAProxy();
        }

        public bool InitiateComunication(X509Certificate othersideCertificate,
            NetTcpBinding binding, EndpointAddress address)
        {
            clientProxy = new ClientService(binding, address);
            /*
             * if(vaProxy.IsValidateCertificate(othersideCertificate)){
             *      messageKey = RandomGenerateKey();
             *      asimetricno definisati messageKey
             *      clientProxy.SetMessageKey(messageKey);
             *      return true;
             * }
             * return false;
             */
            throw new NotImplementedException();
        }

        public bool Pay(byte[] message)
        {
            /*
             * Console.WriteLine(AES128Decryption.Decrypt(messageKey))
             */
            throw new NotImplementedException();
        }

        public void SetMessageKey(byte[,] messageKey)
        {
            //this.messageKey = AES128Decryption.Decrypt(messageKey)
        }

        public X509Certificate Register()
        {
            //return raProxy.Register();
            return null;
        }


        public byte[,] RandomGenerateKey()
        {
            byte[,] retVal = new byte[16, 16];
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    retVal[i, j] = (byte)(new Random().Next(1, 10000));
                }
            }
            return retVal;
        }

    }
}
