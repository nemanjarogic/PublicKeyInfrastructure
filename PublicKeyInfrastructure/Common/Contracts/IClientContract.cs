using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Common.Proxy;

namespace Common.Client
{
    /// <summary>
    /// Service contract on client machine.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IClientContract), SessionMode = SessionMode.Required)]
    public interface IClientContract
    {
        /// <summary>
        /// This method call connected client. Writes message in console.
        /// </summary>
        /// <param name="message">Received message</param>
        [OperationContract]
        void Pay(byte[] message);

        /// <summary>
        /// Returns id of the current session.
        /// </summary>
        [OperationContract]
        String GetSessionId();
        
        /// <summary>
        /// Sends owner certificate in handshake.
        /// </summary>
        /// <param name="cert">Owner certificate.</param>
        /// <returns>Otherside certificate.</returns>
        [OperationContract]
        CertificateDto SendCert(CertificateDto cert);
        
        /// <summary>
        /// Sends session key for encrypt/decrypt messages.
        /// </summary>
        /// <param name="key">Encypted session key in bytes.</param>
        /// <returns>Decrypted session key on otherside (true/false).</returns>
        [OperationContract]
        bool SendKey(byte[] key);

        /// <summary>
        /// Returns pair of session ids.
        /// </summary>
        /// <param name="otherAddress">Host addres of connected client.</param>
        [OperationContract]
        object GetSessionInfo(string otherAddress);

        /// <summary>
        /// Removes session info with parametred address from collecion of connected clients.
        /// If method has been called from RAProxy, client notifies connected clients to removes him from their collection.
        /// </summary>
        /// <param name="clientAddress">Host address of the client for remove.</param>
        [OperationContract]
        void RemoveInvalidClient(string clientAddress);

    }
}
