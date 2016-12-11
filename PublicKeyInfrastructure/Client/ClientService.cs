﻿using Client.Database;
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
        private HashSet<SessionData> clientSessions;
        private X509Certificate2 myCertificate;
        private string hostAddress;
        private string serviceName;
        private IDatabaseWrapper sqliteWrapper;
        private VAProxy vaProxy;
        private RAProxy raProxy;

       
        public ClientService(string hostAddress, IDatabaseWrapper dbWrapper)
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
            InitializeDatabase(dbWrapper);
        }

        public string GetSessionId()
        {
            return OperationContext.Current.SessionId;
        }

        public ClientService()
        {
            //myCertificate = LoadMyCertificate();
            //InitializeDatabase();
        }

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
            foreach (var sessionData in clientSessions)
            {
                if (sessionData.Address.Equals(address))
                {
                    Console.WriteLine("Vec je ostvarena konekcija.");
                    return;
                }
            }

            NetTcpBinding binding = new NetTcpBinding();
            binding.SendTimeout = new TimeSpan(0, 5, 5);
            binding.ReceiveTimeout = new TimeSpan(0, 5, 5);
            binding.OpenTimeout = new TimeSpan(0, 5, 5);
            binding.CloseTimeout = new TimeSpan(0, 5, 5);
            IClientContract serverProxy = new ClientProxy(new EndpointAddress(address), binding, this);
            byte[] messageKey = RandomGenerateKey();

            SessionData sd = new SessionData(new AES128_ECB(messageKey), serverProxy);
            sd.Address = address;

            clientSessions.Add(sd);

            try
            {
                CertificateDto serverCert = serverProxy.SendCert(new CertificateDto(myCertificate));

                if(serverCert == null)
                {
                    Console.WriteLine("Cert nije validan");
                    return;
                }

                RSACryptoServiceProvider publicKey = serverCert.GetCert().PublicKey.Key as RSACryptoServiceProvider;
                byte[] res = publicKey.Encrypt(messageKey, true);
                bool success = serverProxy.SendKey(res);
                if (success)
                {
                    sqliteWrapper.InsertToTable(sd.Address);
                }
                object sessionInfo = serverProxy.GetSessionInfo(hostAddress);

                string[] sessionId = ((string)sessionInfo).Split('|');
                sd.CallbackSessionId = sessionId[0];
                sd.ProxySessionId = sessionId[1];
                clientSessions.Add(sd);
            }
            catch (EndpointNotFoundException)
            {
                Console.WriteLine("Druga strana nije aktivna");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        public void CallPay(byte[] message, string address)
        {
            string serviceId = OperationContext.Current.SessionId;
            foreach (SessionData sd in clientSessions)
            {
                if (sd.Address.Equals(address))
                {
                    try
                    {
                        sd.Proxy.Pay(sd.AesAlgorithm.Encrypt(message));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    return;
                }
            }
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
            //retVal = raProxy.RegisterClient(hostAddress);
            X509Certificate2 retCert = null;
            CertificateDto certDto = null;
            certDto = raProxy.RegisterClient(hostAddress);
            retCert = certDto.GetCert();

            return retCert;
        }

        public CertificateDto SendCert(CertificateDto certDto)
        {
            if(!vaProxy.isCertificateValidate(certDto.GetCert()))
            {
                return null;
            }
            IClientContract otherSide = OperationContext.Current.GetCallbackChannel<IClientContract>();
            string callbackSession = otherSide.GetSessionId();
            string proxySession = OperationContext.Current.SessionId;

            /*Provjeri cert*/
            clientSessions.Add(new SessionData(null, otherSide, callbackSession, proxySession));

            return new CertificateDto(myCertificate);
        }

        public bool SendKey(byte[] key)
        {
            /*Ako je kljuc validan vrati true*/
            SessionData sd = GetSession(OperationContext.Current.SessionId);

            if (sd != null)
            {
                RSACryptoServiceProvider privateKey = myCertificate.PrivateKey as RSACryptoServiceProvider;
                byte[] result = privateKey.Decrypt(key, true);
                sd.AesAlgorithm = new AES128_ECB(result);
            }
            return true;
        }

        private SessionData GetSession(string sessionId)
        {
            foreach (SessionData sd in clientSessions)
            {
                if (sd.IsValidSession(sessionId))
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
            if (sd != null)
            {
                sd.Address = otherAddress;
                sqliteWrapper.InsertToTable(sd.Address);
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
