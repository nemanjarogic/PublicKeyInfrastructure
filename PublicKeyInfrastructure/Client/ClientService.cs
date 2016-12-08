using Common.Client;
using Cryptography.AES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
        private X509Certificate2 myCertificate;

        public ClientService() { }

        public string GetSessionId(){
            return OperationContext.Current.SessionId;
        }
        
        public ClientService(NetTcpBinding binding, EndpointAddress address)
        {
            clientSessions = new Dictionary<string, SessionData>();
            myCertificate = LoadMyCertificate();

            vaProxy = new VAProxy(); /*ucitati adresu i binding i proslediti u konstuktor*/
            raProxy = new RAProxy();
        }

        #region Handshake
        public void StartComunication(string address)
        {
            IClientContract serverProxy = new ClientProxy(new EndpointAddress(address), new NetTcpBinding(), this);
            string serverSessionId = serverProxy.GetSessionId();
            messageKey = RandomGenerateKey();
            clientSessions.Add(serverSessionId, new SessionData(new AES128_ECB(messageKey), serverProxy, serverSessionId));
            serverProxy.InitiateComunication(myCertificate);
        }

        public void InitiateComunication(X509Certificate2 othersideCertificate)
        {
            
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string otherSideSessionId = otherSide.GetSessionId();
            if (vaProxy.isCertificateValidate(othersideCertificate))
            {
                clientSessions.Add(otherSideSessionId, new SessionData(null, otherSide, otherSideSessionId));
                otherSide.AcceptComunication(myCertificate);
            }
        }

        public void AcceptComunication(X509Certificate2 othersideCertificate)
        {
            string serviceId = OperationContext.Current.SessionId;
            SessionData otherside;
            clientSessions.TryGetValue(serviceId, out otherside);

            if(vaProxy.isCertificateValidate(othersideCertificate) && otherside != null)
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

        public X509Certificate2 Register(string subjectName)
        {
            return raProxy.RegisterClient(subjectName);
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

        X509Certificate2 LoadMyCertificate()
        {
            X509Certificate2 retVal = new X509Certificate2();

            string subjectName = WindowsIdentity.GetCurrent().Name;
            try
            {
                retVal.Import("../../Certificates/" + subjectName + ".cer");
            }
            catch
            {
                retVal = raProxy.RegisterClient(subjectName);
            }
            return retVal;
        }
    }
}
