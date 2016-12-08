using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Cryptography.AES;
using Common.Client;

namespace Client
{
    class SessionData
    {
        public AES128_ECB AesAlgorithm { get; set; }
        public IClientContract Proxy { get; set; }
        public string ProxySessionId { get; set; }
        public string CallbackSessionId { get; set; }
        public bool IsSuccessfull { get; set; }
        public string Address { get; set; }

        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
        }

        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy, string proxySession, string callbackSession)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
            ProxySessionId = proxySession;
            CallbackSessionId = callbackSession;
        }

        public bool IsValidSession(string sessionId)
        {
            return sessionId.Equals(CallbackSessionId) || sessionId.Equals(ProxySessionId);
        }

    }
}
