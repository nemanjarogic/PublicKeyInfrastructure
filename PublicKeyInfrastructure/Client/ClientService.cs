using Client.Database;
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
        private HashSet<SessionData> clientSessions;
        private X509Certificate2 myCertificate;
        private string hostAddress;
        private string serviceName;
        private IDatabaseWrapper sqliteWrapper;
        private VAProxy vaProxy;
        private RAProxy raProxy;

        public ClientService(string hostAddress) 
        {
            NetTcpBinding raBinding = new NetTcpBinding();
            raBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            string raAddress = "net.tcp://localhost:10002/RegistrationAuthorityService";

            NetTcpBinding vaBinding = new NetTcpBinding();
            vaBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            string vaAddress = "net.tcp://localhost:10003/ValidationAuthorityService";
            vaProxy = new VAProxy(vaAddress, vaBinding);
            raProxy = new RAProxy(raAddress, raBinding);

            clientSessions = new HashSet<SessionData>();
            this.hostAddress = hostAddress;
            myCertificate = LoadMyCertificate(); //myCertificate = new X509Certificate2(@"D:\Fakultet\Master\Blok3\Security\WCFClient.pfx", "12345");
            InitializeDatabase();
        }

        public string GetSessionId(){
            return OperationContext.Current.SessionId;
        }
        
        public ClientService()
        {
            myCertificate = LoadMyCertificate();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            string subjectName = WindowsIdentity.GetCurrent().Name;
            string port = hostAddress.Split(':')[2].Split('/')[0];
            string subjName = subjectName.Split('\\')[0];
            serviceName = subjName + port;

            sqliteWrapper = new SQLiteWrapper();
            sqliteWrapper.CreateDatabase("Connections");
            sqliteWrapper.ConnectToDatabase("Connections");
            sqliteWrapper.CreateTable(serviceName);
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

            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = new TimeSpan(0, 0, 5);
            binding.ReceiveTimeout = new TimeSpan(0, 0, 5);
            binding.OpenTimeout = new TimeSpan(0, 0, 5);
            binding.CloseTimeout = new TimeSpan(0, 0, 5);
            IClientContract serverProxy = new ClientProxy(new EndpointAddress(address), binding, this);
            byte[] messageKey = RandomGenerateKey();

            SessionData sd = new SessionData(new AES128_ECB(messageKey), serverProxy);
            sd.Address = address;

            clientSessions.Add(sd);

            try
            {
            X509Certificate2 serverCert = serverProxy.SendCert(null);

            RSACryptoServiceProvider publicKey = myCertificate.PublicKey.Key as RSACryptoServiceProvider;
            bool success = serverProxy.SendKey(publicKey.Encrypt(messageKey, true)); 
            if (success)
            {
                sqliteWrapper.InsertToTable(serviceName, sd.Address);
            }
            object sessionInfo = serverProxy.GetSessionInfo(hostAddress);

            string[] sessionId = ((string)sessionInfo).Split('|');
            sd.CallbackSessionId = sessionId[0];
            sd.ProxySessionId = sessionId[1];
            clientSessions.Add(sd);
        }
            catch(EndpointNotFoundException)
            {
                Console.WriteLine("Druga strana nije aktivna");
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        public void CallPay(byte[] message, string address)
        {
            string serviceId = OperationContext.Current.SessionId;
            foreach(SessionData sd in clientSessions)
            {
                if(sd.Address.Equals(address))
                {
                    try
                    {
                    sd.Proxy.Pay(sd.AesAlgorithm.Encrypt(message));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    return;
                }
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

            retVal = raProxy.RegisterClient(hostAddress);

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
                sqliteWrapper.InsertToTable(serviceName, sd.Address);
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
