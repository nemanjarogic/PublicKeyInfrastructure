using Common.Server;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    public class CertificationAuthorityService : ICertificationAuthorityContract
    {
        #region Fields

        private HashSet<X509Certificate2> activeCertificates;
        private HashSet<X509Certificate2> revocationList;
        private AsymmetricKeyParameter caPrivateKey = null;
        private X509Certificate2 caCertificate = null;
        private readonly string caSubjectName;

        #endregion

        #region Constructor

        public CertificationAuthorityService()
        {
            activeCertificates = new HashSet<X509Certificate2>();
            revocationList = new HashSet<X509Certificate2>();

            caSubjectName = "CN=PKI_CA";
            PrepareCAService();
        }

        #endregion

        #region Public methods

        public X509Certificate2 GenerateCertificate(string subjectName)
        {
            return CertificateHandler.GenerateSelfSignedCertificate(subjectName, "CN=" + caSubjectName, caPrivateKey); ;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            return true;
        }

        #endregion 

        #region Private methods

        private void PrepareCAService()
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates;

            bool isCertFound = false;
            foreach(var cert in certs)
            {
                if (cert.SubjectName.Equals(caSubjectName))
                {
                    isCertFound = true;
                    caCertificate = cert;
                    //caPrivateKey = cert.PrivateKey;
                    break;
                }
            }


            if (!isCertFound)
            {
                caCertificate = CertificateHandler.GenerateCACertificate(caSubjectName, ref caPrivateKey);
            }
        }

        #endregion 
    }
}
