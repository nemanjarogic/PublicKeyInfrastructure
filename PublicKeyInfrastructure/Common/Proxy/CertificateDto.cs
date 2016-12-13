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

        public CertificateDto(X509Certificate2 certParam, bool includePrivateKey=true)
        {
            this.cert = certParam;
            //if (certParam != null && certParam.HasPrivateKey)
            if(includePrivateKey)
                stringPrivateKey = certParam.PrivateKey.ToXmlString(true);
            //stringPrivateKey = null;
        }

        #endregion

        #region Public methods

        public X509Certificate2 GetCert(bool includePrivateKey=true)
        {
            if (cert == null)
                return cert;

            if (includePrivateKey)
            {
                AsymmetricAlgorithm privateKey = new RSACryptoServiceProvider();
                privateKey.FromXmlString(stringPrivateKey);
                cert.PrivateKey = privateKey;
            }
            return cert;
        }

        private string GetStringPrivateKey()
        {
            return stringPrivateKey;
        }

        #endregion
    }
}
