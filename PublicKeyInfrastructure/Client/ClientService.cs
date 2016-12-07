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
    public class SessionData
    {
        public AES128_ECB AesAlgorithm { get; set; }
        public IClientContract Proxy { get; set; }
        public string SessionId { get; set; }

        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy, string sessionId)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
            SessionId = sessionId;
        }
    }
    public class ClientService : DuplexChannelFactory<IClientContract>, IClientContract, IDisposable
    {
        private Dictionary<string, SessionData> tempProxySessions;
        private Dictionary<string, SessionData> clientProxySessions;
        private VAProxy vaProxy;
        private RAProxy raProxy;
        private byte[] messageKey;
        private X509Certificate myCertificate;

        private IClientContract proxy;

        public string GetSessionId(){
            return OperationContext.Current.SessionId;
        }

        public ClientService(NetTcpBinding binding, EndpointAddress address, object callback)
            : base(callback, binding, address)
        {
            proxy = this.CreateChannel();
            tempProxySessions = new Dictionary<string, SessionData>();
            clientProxySessions = new Dictionary<string, SessionData>();
            myCertificate = null /*Ucitati nekako*/;

            vaProxy = new VAProxy();
            raProxy = new RAProxy();
        }

        public void StartComunication(NetTcpBinding binding, EndpointAddress address)
        {
            IClientContract serverProxy = new ClientService(binding, address, this);
            string serverSessionId = serverProxy.GetSessionId();
            messageKey = RandomGenerateKey();
            tempProxySessions.Add(serverSessionId, new SessionData(new AES128_ECB(messageKey), serverProxy, serverSessionId));
            serverProxy.InitiateComunication(myCertificate);
        }

        public void InitiateComunication(X509Certificate othersideCertificate)
        {
            
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string otherSideSessionId = otherSide.GetSessionId();
            /*if (vaProxy.IsValidate(othersideCertificate))*/
            {
                tempProxySessions.Add(otherSideSessionId, new SessionData(new AES128_ECB(messageKey), otherSide, otherSideSessionId));
                otherSide.AcceptComunication(myCertificate);
            }
        }

        public void AcceptComunication(X509Certificate othersideCertificate)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            tempProxySessions.TryGetValue(serviceId, out otherside);

            /*if(vaProxy.IsValidate(othersideCertificate) && otherside != null)*/
            {

                otherside.Proxy.SetMessageKey(messageKey);
            }
        }

        public void SetMessageKey(byte[] messageKey)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            tempProxySessions.TryGetValue(serviceId, out otherside);
            if (otherside != null)
            {
                otherside.Proxy.ReadyForMessaging();
            }
        }

        public void ReadyForMessaging()
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            tempProxySessions.TryGetValue(serviceId, out otherside);
            if (otherside != null)
            {
                clientProxySessions.Add(otherside.SessionId, otherside);
                otherside.Proxy.Pay(null);
            }
        }

        public void Pay(byte[] message)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientProxySessions.TryGetValue(serviceId, out otherside);
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
