using Common.Client;
using Cryptography.AES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public class ClientService : IClientContract
    {
        private Dictionary<string, SessionData> clientSessions;
        private VAProxy vaProxy;
        private RAProxy raProxy;
        private byte[] messageKey;
        private X509Certificate myCertificate;

        public string GetSessionId(){
            return OperationContext.Current.SessionId;
        }

        #region Handshake
        public ClientService(NetTcpBinding binding, EndpointAddress address)
        {
            clientSessions = new Dictionary<string, SessionData>();
            myCertificate = null /*Ucitati nekako*/;

            vaProxy = new VAProxy();
            raProxy = new RAProxy();
        }

        public void StartComunication(NetTcpBinding binding, EndpointAddress address)
        {
            IClientContract serverProxy = new ClientProxy(address, binding, this);
            string serverSessionId = serverProxy.GetSessionId();
            messageKey = RandomGenerateKey();
            clientSessions.Add(serverSessionId, new SessionData(new AES128_ECB(messageKey), serverProxy, serverSessionId));
            serverProxy.InitiateComunication(myCertificate);
        }

        public void InitiateComunication(X509Certificate othersideCertificate)
        {
            
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string otherSideSessionId = otherSide.GetSessionId();
            /*if (vaProxy.IsValidate(othersideCertificate))*/
            {
                clientSessions.Add(otherSideSessionId, new SessionData(null, otherSide, otherSideSessionId));
                otherSide.AcceptComunication(myCertificate);
            }
        }

        public void AcceptComunication(X509Certificate othersideCertificate)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientSessions.TryGetValue(serviceId, out otherside);

            /*if(vaProxy.IsValidate(othersideCertificate) && otherside != null)*/
            {

                otherside.Proxy.SetMessageKey(otherside.AesAlgorithm.Key); //Treba ga i kriptovati
            }
        }

        public void SetMessageKey(byte[] messageKey)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientSessions.TryGetValue(serviceId, out otherside);
            if (otherside != null)
            {
                otherside.AesAlgorithm = new AES128_ECB(messageKey); //Dekriptuj ga
                otherside.IsSuccessfull = true;
                otherside.Proxy.ReadyForMessaging();
            }
        }

        public void ReadyForMessaging()
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientSessions.TryGetValue(serviceId, out otherside);
            if (otherside != null)
            {
                otherside.IsSuccessfull = true;
                //otherside.Proxy.Pay(null);
            }
        }
        #endregion

        public void Pay(byte[] message)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientSessions.TryGetValue(serviceId, out otherside);
            if (otherside != null)
            {
                Console.WriteLine(otherside.SessionId + " paid: " + System.Text.Encoding.UTF8.GetString(otherside.AesAlgorithm.Decrypt(message)));
            }
        }

        public X509Certificate Register()
        {
            //return raProxy.Register();
            return null;
        }

        public byte[] RandomGenerateKey()
        {
            byte[] retVal = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                retVal[i] = (byte)(new Random().Next(1, 10000));
            }
            return retVal;
        }
    }
}
