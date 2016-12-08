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

        public void Pay(byte[] message)
        {
            this.proxy.Pay(message);
        }

        public void StartComunication(string address)
        {
            this.proxy.StartComunication(address);
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


        public X509Certificate2 SendCert(X509Certificate2 cert)
        {
            return this.proxy.SendCert(cert);
        }

        public bool SendKey(byte[] key)
        {
            return this.proxy.SendKey(key);
        }


        public object GetSessionInfo(string address)
        {
            return this.proxy.GetSessionInfo(address);
        }

        public void CallPay(byte[] message, string address)
        {
            this.proxy.CallPay(message, address);
        }
    }
}
