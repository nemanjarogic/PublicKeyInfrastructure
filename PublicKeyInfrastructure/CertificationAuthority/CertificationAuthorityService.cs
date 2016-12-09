using Common.Server;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        private readonly string CA_SUBJECT_NAME;
        private readonly string PFX_PATH;
        private readonly string PFX_PASSWORD;
        private readonly string CERT_FOLDER_PATH;

        #endregion

        #region Constructor

        public CertificationAuthorityService()
        {
            activeCertificates = new HashSet<X509Certificate2>();
            revocationList = new HashSet<X509Certificate2>();

            CA_SUBJECT_NAME = "CN=PKI_CA";
            PFX_PATH = @"..\..\SecurityStore\PKI_CA.pfx";
            PFX_PASSWORD = "123";
            CERT_FOLDER_PATH = @"..\..\SecurityStore\";

            PrepareCAService();
        }

        #endregion

        #region Public methods

        public X509Certificate2 GenerateCertificate(string address)
        {
            X509Certificate2 newCertificate = null;
            newCertificate = CertificateHandler.GenerateAuthorizeSignedCertificate(address, "CN=" + CA_SUBJECT_NAME, caPrivateKey);
            if (newCertificate != null)
            {
                activeCertificates.Add(newCertificate);
            }

            return newCertificate;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            return true;
        }

        public FileStream GetFileStreamOfCertificate(string certFileName)
        {
            return new FileStream(CERT_FOLDER_PATH + @"\" + certFileName + ".pfx", FileMode.Open, FileAccess.Read);
        }

        public bool SaveCertificateToBackupDisc(X509Certificate2 certificate, FileStream stream, string certFileName)
        {
            //save file to disk
            var fileStream = File.Create(CERT_FOLDER_PATH + @"\" + certFileName + ".pfx");
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            fileStream.Close();

            //add cert to list
            activeCertificates.Add(certificate);

            return true;
        }

        public object GetModel()
        {
            throw new NotImplementedException();
            //TODO: implementirati getModel na CA servisu 
        }

        public bool SetModel(object param)
        {
            throw new NotImplementedException();
            //TODO: implementirati setModel na CA servisu
        }

        #endregion 

        #region Private methods

        /// <summary>
        /// Prepare certification authority service for use.
        /// Load information about CA and active certificates in system.
        /// </summary>
        private void PrepareCAService()
        {
            bool isPfxCreated = true;
            bool isCertFound = false;
            X509Certificate2Collection collection = new X509Certificate2Collection();

            try
            {
                // Try to import pfx file for the CA(Certification authority)
                collection.Import(PFX_PATH, PFX_PASSWORD, X509KeyStorageFlags.Exportable);
            }
            catch
            {
                isPfxCreated = false;
            }

            if(isPfxCreated)
            {
                foreach (X509Certificate2 cert in collection)
                {
                    if (cert.SubjectName.Name.Equals(CA_SUBJECT_NAME))
                    {
                        isCertFound = true;
                        caCertificate = cert;
                        caPrivateKey = DotNetUtilities.GetKeyPair(cert.PrivateKey).Private;

                        break;
                    }
                }
            }

            if (!isCertFound)
            {
                // if PFX for the CA isn't created generate certificate and PFX for the CA
                caCertificate = CertificateHandler.GenerateCACertificate(CA_SUBJECT_NAME, ref caPrivateKey);
            }
        }

        #endregion 

    }
}
