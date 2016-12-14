using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common.Client;
using Cryptography.AES;

namespace Client
{
    /// <summary>
    /// Class that describes connected client session.
    /// </summary>
    class SessionData
    {
        /// <summary>
        /// AES algorithm with unique key that client uses in communication with other side.
        /// </summary>
        public AES128_ECB AesAlgorithm { get; set; }

        /// <summary>
        /// Other side proxy.
        /// </summary>
        public IClientContract Proxy { get; set; }

        /// <summary>
        /// Session Id in request way.
        /// </summary>
        public string ProxySessionId { get; set; }

        /// <summary>
        /// Session Id in response way.
        /// </summary>
        public string CallbackSessionId { get; set; }

        /// <summary>
        /// Host address of the other side service.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Constructor without parameters.
        /// </summary>
        public SessionData() { }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="aesAlgorithm">Sets AES algorithm.</param>
        /// <param name="proxy">Sets proxy.</param>
        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
        }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="proxySession">Sets session id in request way.</param>
        /// <param name="callbackSession">Sets session id in response way.</param>
        public SessionData(AES128_ECB aesAlgorithm, IClientContract proxy, string proxySession, string callbackSession)
        {
            AesAlgorithm = aesAlgorithm;
            Proxy = proxy;
            ProxySessionId = proxySession;
            CallbackSessionId = callbackSession;
        }

        /// <summary>
        /// Checks affiliations of the parametred session id in session id pair.
        /// </summary>
        public bool IsValidSession(string sessionId)
        {
            return sessionId.Equals(CallbackSessionId) || sessionId.Equals(ProxySessionId);
        }

    }
}
