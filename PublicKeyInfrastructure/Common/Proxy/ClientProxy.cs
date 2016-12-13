using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Client;
using System.Security.Cryptography.X509Certificates;
using Common.Proxy;

namespace Client
{
    public class ClientProxy : DuplexChannelFactory<IClientContract>, IClientContract, IDisposable
    {
        private IClientContract proxy;

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

        public CertificateDto SendCert(CertificateDto cert)
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

        public void RemoveInvalidClient(string clientAddress)
        {
            this.proxy.RemoveInvalidClient(clientAddress);
        }


        public Dictionary<int, string> GetClients()
        {
            return this.proxy.GetClients();
        }
    }
}
