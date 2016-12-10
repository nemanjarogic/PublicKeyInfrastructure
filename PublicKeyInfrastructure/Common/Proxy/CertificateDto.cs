using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common.Proxy
{
    [DataContract]
    public class CertificateDto
    {
        #region Fields
        
        [DataMember]
        private X509Certificate2 cert;

        [DataMember]
        private string stringPrivateKey;

        #endregion

        #region Constructors

        public CertificateDto(X509Certificate2 certParam, AsymmetricAlgorithm pKeyParam)
        {
            this.cert = certParam;
            stringPrivateKey = pKeyParam.ToXmlString(true);
        }

        #endregion

        #region Public methods

        public X509Certificate2 GetCert()
        {
            return cert;
        }

        public string GetStringPrivateKey()
        {
            return stringPrivateKey;
        }

        #endregion
    }
}
