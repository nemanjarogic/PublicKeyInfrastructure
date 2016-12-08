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
        public string SessionId { get; set; }
        public bool IsSuccessfull { get; set; }

        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy, string sessionId)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
            SessionId = sessionId;
            IsSuccessfull = false;
        }
    }
}
