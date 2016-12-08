using Common.Client;
using Cryptography.AES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientService : IClientContract
    {
        //private Dictionary<string, SessionData> clientSessions;
        private HashSet<SessionData> clientSessions;
        private VAProxy vaProxy;
        private RAProxy raProxy;
        private X509Certificate2 myCertificate;
        private string hostAddress;

        public ClientService() { }

        public ClientService(string hostAddress) 
        {
            //clientSessions = new Dictionary<string, SessionData>();
            clientSessions = new HashSet<SessionData>();
            this.hostAddress = hostAddress;
            myCertificate = new X509Certificate2(@"D:\Fakultet\Master\Blok3\Security\WCFClient.pfx", "12345");
        }

        public string GetSessionId(){
            return OperationContext.Current.SessionId;
        }
        
        public ClientService(NetTcpBinding binding, EndpointAddress address)
        {
            //clientSessions = new Dictionary<string, SessionData>();
            myCertificate = LoadMyCertificate();

            vaProxy = new VAProxy(); /*ucitati adresu i binding i proslediti u konstuktor*/
            raProxy = new RAProxy();
        }

        public void StartComunication(string address)
        {
            foreach(var sessionData in clientSessions)
            {
                if(sessionData.Address.Equals(address))
                {
                    Console.WriteLine("Vec je ostvarena konekcija.");
                    return;
                }
            }
            IClientContract serverProxy = new ClientProxy(new EndpointAddress(address), new NetTcpBinding(), this);
            byte[] messageKey = RandomGenerateKey();

            SessionData sd = new SessionData(new AES128_ECB(messageKey), serverProxy);
            sd.Address = address;

            clientSessions.Add(sd);

            X509Certificate2 serverCert = serverProxy.SendCert(null);

            RSACryptoServiceProvider publicKey = myCertificate.PublicKey.Key as RSACryptoServiceProvider;
            bool success = serverProxy.SendKey(publicKey.Encrypt(messageKey, true)); 

            object sessionInfo = serverProxy.GetSessionInfo(hostAddress);

            string[] sessionId = ((string)sessionInfo).Split('|');
            sd.CallbackSessionId = sessionId[0];
            sd.ProxySessionId = sessionId[1];
            clientSessions.Add(sd);
        }

        public void CallPay(byte[] message, string address)
        {
            string serviceId = OperationContext.Current.SessionId;
            foreach(SessionData sd in clientSessions)
            {
                if(sd.Address.Equals(address))
                {
                    sd.Proxy.Pay(sd.AesAlgorithm.Encrypt(message));
                    return;
                }
            }

            //clientSessions.TryGetValue(serviceId, out otherside);
            //if (otherside != null)
            {
               // Console.WriteLine(otherside.ProxySessionId + " paid: " + System.Text.Encoding.UTF8.GetString(otherside.AesAlgorithm.Decrypt(message)));
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

        public X509Certificate2 LoadMyCertificate()
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

        public X509Certificate2 SendCert(X509Certificate2 cert)
        {
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string callbackSession = otherSide.GetSessionId();
            string proxySession = OperationContext.Current.SessionId;

            /*Provjeri cert*/
            clientSessions.Add(new SessionData(null, otherSide, callbackSession, proxySession));

            return null;
        }

        public bool SendKey(byte[] key)
        {
            /*Ako je kljuc validan vrati true*/
            SessionData sd = GetSession(OperationContext.Current.SessionId);
            if(sd != null)
            {
                RSACryptoServiceProvider privateKey = myCertificate.PrivateKey as RSACryptoServiceProvider;
                sd.AesAlgorithm = new AES128_ECB(privateKey.Decrypt(key, true));
            }
            return true;
        }

        private SessionData GetSession(string sessionId)
        {
            foreach(SessionData sd in clientSessions)
            {
                if(sd.IsValidSession(sessionId))
                {
                    return sd;
                }
            }
            return null;
        }

        public object GetSessionInfo(string otherAddress)
        {
            string sessionId = OperationContext.Current.SessionId;
            SessionData sd = GetSession(sessionId);

            Console.WriteLine("Session is opened");
            if(sd != null)
            {
                sd.Address = otherAddress;
                return string.Format("{0}|{1}", sd.CallbackSessionId, sd.ProxySessionId);
            }
            return null;
        }

        public void Pay(byte[] message)
        {
            string sessionId = OperationContext.Current.SessionId;
            SessionData sd = GetSession(sessionId);
            Console.WriteLine(sd.Address + " paid: " + System.Text.Encoding.UTF8.GetString(sd.AesAlgorithm.Decrypt(message)));
        }
    }
}
