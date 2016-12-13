using Client.Database;
using Common.Client;
using Common.Proxy;
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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientService : IClientContract
    {
        private Dictionary<string, SessionData> clientSessions;
        private X509Certificate2 myCertificate;
        private string hostAddress;
        private string serviceName;
        private IDatabaseWrapper sqliteWrapper;
        private VAProxy vaProxy;
        private RAProxy raProxy;
        private int tempSessionNum = 0;
        private object objLock = new Object();

        public ClientService(string hostAddress, IDatabaseWrapper dbWrapper)
        {
            NetTcpBinding raBinding = new NetTcpBinding();
            raBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            //string raAddress = "net.tcp://10.1.212.117:10002/RegistrationAuthorityService";
            string raAddress = "net.tcp://localhost:10002/RegistrationAuthorityService";

            NetTcpBinding vaBinding = new NetTcpBinding();
            vaBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            //string vaAddress = "net.tcp://10.1.212.117:10003/ValidationAuthorityService";
            string vaAddress = "net.tcp://localhost:10003/ValidationAuthorityService";
            vaProxy = new VAProxy(vaAddress, vaBinding);
            raProxy = new RAProxy(raAddress, raBinding);
            clientSessions = new Dictionary<string, SessionData>();
            this.hostAddress = hostAddress;
            myCertificate = LoadMyCertificate();
            InitializeDatabase(dbWrapper);
        }

        public ClientService() { }

        private void InitializeDatabase(IDatabaseWrapper dbWrapper)
        {
            string subjectName = WindowsIdentity.GetCurrent().Name;
            string port = hostAddress.Split(':')[2].Split('/')[0];
            string subjName = subjectName.Replace('\\', '_').Replace('-', '_');
            serviceName = subjName + port;

            sqliteWrapper = dbWrapper;
            sqliteWrapper.CreateDatabase("Connections" + port);
            sqliteWrapper.ConnectToDatabase();
            sqliteWrapper.CreateTable(serviceName);
        }

        public void StartComunication(string address)
        {
            if (clientSessions.ContainsKey(address))
            {
                Console.WriteLine("You are already connected to client: {0}", address);
                return;
            }

            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = new TimeSpan(0, 5, 5);
            binding.ReceiveTimeout = new TimeSpan(0, 5, 5);
            binding.OpenTimeout = new TimeSpan(0, 5, 5);
            binding.CloseTimeout = new TimeSpan(0, 5, 5);
            IClientContract serverProxy = new ClientProxy(new EndpointAddress(address), binding, this);

            byte[] sessionKey = RandomGenerateKey();
            SessionData sd = new SessionData() { AesAlgorithm = new AES128_ECB(sessionKey), Proxy = serverProxy, Address = address };

            CertificateDto serverCert = serverProxy.SendCert(new CertificateDto(myCertificate));

            if (serverCert == null)
            {
                Console.WriteLine("My certificate is not valid!");
                return;
            }

            byte[] encryptedSessionKey = null;
            try
            {
                RSACryptoServiceProvider publicKey = (RSACryptoServiceProvider)serverCert.GetCert().PublicKey.Key;

                if (publicKey != null)
                {
                    encryptedSessionKey = publicKey.Encrypt(sessionKey, true);
                }
                else
                {
                    Console.WriteLine("Error, public key is null");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            bool success = serverProxy.SendKey(encryptedSessionKey);
            if (success)
            {
                sqliteWrapper.InsertToTable(sd.Address);

                object sessionInfo = serverProxy.GetSessionInfo(hostAddress);
                if (sessionInfo != null)
                {
                    string[] sessionId = ((string)sessionInfo).Split('|');
                    sd.CallbackSessionId = sessionId[0];
                    sd.ProxySessionId = sessionId[1];
                    lock (objLock)
                    {
                        clientSessions.Add(sd.Address, sd);
                    }
                }
            }
            else
            {
                Console.WriteLine("Starting communication failed!");
            }
        }

        public CertificateDto SendCert(CertificateDto certDto)
        {
            if (!vaProxy.isCertificateValidate(certDto.GetCert()))
            {
                return null;
            }
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string callbackSession = otherSide.GetSessionId();
            string proxySession = OperationContext.Current.SessionId;

            SessionData newSd = new SessionData(null, otherSide, callbackSession, proxySession);
            newSd.Address = string.Format("temp{0}", tempSessionNum++);
            clientSessions.Add(newSd.Address, newSd);

            return new CertificateDto(myCertificate);
        }

        public bool SendKey(byte[] key)
        {
            SessionData sd = GetSession(OperationContext.Current.SessionId);
            if (sd != null)
            {
                try
                {
                    RSACryptoServiceProvider privateKey = (RSACryptoServiceProvider)myCertificate.PrivateKey;
                    byte[] result = privateKey.Decrypt(key, true);
                    sd.AesAlgorithm = new AES128_ECB(result);
                    return true;
                }
                catch (Exception e)
                {
                    lock (objLock)
                    {
                        clientSessions.Remove(sd.Address);
                    }
                    Console.WriteLine(e.Message);
                }
            }
            return false;
        }

        public X509Certificate2 LoadMyCertificate()
        {
            X509Certificate2 retCert = null;
            CertificateDto certDto = null;
            certDto = raProxy.RegisterClient(hostAddress);
            retCert = certDto.GetCert();

            return retCert;
        }

        private SessionData GetSession(string sessionId)
        {
            foreach (var sd in clientSessions)
            {
                if (sd.Value.IsValidSession(sessionId))
                {
                    return sd.Value;
                }
            }
            return null;
        }

        public string GetSessionId()
        {
            return OperationContext.Current.SessionId;
        }

        public object GetSessionInfo(string otherAddress)
        {
            string sessionId = OperationContext.Current.SessionId;
            SessionData sd = GetSession(sessionId);
            if (sd != null)
            {
                lock (objLock)
                {
                    clientSessions.Remove(sd.Address);
                    sd.Address = otherAddress;
                    clientSessions.Add(sd.Address, sd);
                }
                sqliteWrapper.InsertToTable(sd.Address);

                Console.WriteLine("Session is opened");

                return string.Format("{0}|{1}", sd.CallbackSessionId, sd.ProxySessionId);
            }
            return null;
        }

        public void Pay(byte[] message)
        {
            string sessionId = OperationContext.Current.SessionId;
            SessionData sd = GetSession(sessionId);
            Console.WriteLine(string.Format("From: {0}, message: {1}", sd.Address, System.Text.Encoding.UTF8.GetString(sd.AesAlgorithm.Decrypt(message))));
        }

        public void CallPay(byte[] message, string address)
        {
            SessionData session = null;

            if (clientSessions.TryGetValue(address, out session))
            {
                try
                {
                    session.Proxy.Pay(session.AesAlgorithm.Encrypt(message));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public byte[] RandomGenerateKey()
        {
            byte[] retVal = new byte[16];
            Random rnd = new Random(Environment.TickCount);
            for (int i = 0; i < 16; i++)
            {
                retVal[i] = (byte)rnd.Next();
            }
            return retVal;
        }

        public void RemoveInvalidClient(string clientAddress)
        {
            //Kada PKI pogodi klijenta ciji je sertifikat istekao, 
            //on javlja svim povezanim da ga obrisu iz liste konektovanih ali prazni i svoju listu konektovanih.
            if (clientAddress.Equals(hostAddress))
            {
                foreach (KeyValuePair<string, SessionData> connectedClient in clientSessions)
                {
                    connectedClient.Value.Proxy.RemoveInvalidClient(hostAddress);
                }
                lock (objLock)
                {
                    clientSessions.Clear();
                }
            }
            //U else ulazi kada metodu pozove proksi iz klijenta ciji je sertifikat istekao
            else
            {
                lock (objLock)
                {
                    clientSessions.Remove(clientAddress);
                }
            }
        }

        public Dictionary<int, string> GetClients()
        {
            Dictionary<int, string> retVal = new Dictionary<int, string>();
            int num = 1;
            foreach(var key in clientSessions.Keys)
            {
                retVal.Add(num++, key);
            }
            return retVal;
        }
    }
}
