using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Client;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    class ClientProxy : DuplexChannelFactory<IClientContract>, IClientContract, IDisposable
    {
        IClientContract proxy;

        public ClientProxy(EndpointAddress address, NetTcpBinding binding, object callback)
            : base(callback, binding, address)
        {
            proxy = this.CreateChannel();
        }

        public void InitiateComunication(X509Certificate2 othersideCertificate)
        {
            this.proxy.InitiateComunication(othersideCertificate);
        }

        public void SetMessageKey(byte[] messageKey)
        {
            this.proxy.SetMessageKey(messageKey);
        }

        public void Pay(byte[] message)
        {
            this.proxy.Pay(message);
        }

        public void StartComunication(string address)
        {
            this.StartComunication(address);
        }

        public void AcceptComunication(X509Certificate2 myCertificate)
        {
            this.proxy.AcceptComunication(myCertificate);
        }

        public void ReadyForMessaging()
        {
            this.proxy.ReadyForMessaging();
        }

        public string GetSessionId()
        {
            return this.proxy.GetSessionId();
        }

        public X509Certificate Register()
        {
            throw new NotImplementedException();
        }

        public byte[] RandomGenerateKey()
        {
            throw new NotImplementedException();
        }


        public X509Certificate2 Register(string subjectName)
        {
            throw new NotImplementedException();
        }

        public X509Certificate2 LoadMyCertificate()
        {
            throw new NotImplementedException();
        }
    }
}
