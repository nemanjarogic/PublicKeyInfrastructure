using Common.Proxy;
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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class CertificationAuthorityService : ICertificationAuthorityContract
    {
        #region Fields

        #region CA model
        private HashSet<X509Certificate2> activeCertificates;
        private HashSet<X509Certificate2> revocationList;
        private Dictionary<string, string> clientDict;
        #endregion

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
            clientDict = new Dictionary<string, string>();

            CA_SUBJECT_NAME = "CN=PKI_CA";
            PFX_PATH = @"..\..\SecurityStore\PKI_CA.pfx";
            PFX_PASSWORD = "123";
            CERT_FOLDER_PATH = @"..\..\SecurityStore\";

            PrepareCAService();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generate new certificate with specified subject name.
        /// If certificate with given subject name already exist this action is forbiden. 
        /// </summary>
        /// <param name="subject">Subject name</param>
        /// <param name="address">Host address of client</param>
        /// <returns></returns>
        public CertificateDto GenerateCertificate(string subject, string address)
        {
            CertificateDto retVal = null;
            X509Certificate2 newCertificate = null;

            newCertificate = IsCertificatePublished(subject);
            if (newCertificate == null)
            {
                newCertificate = CertificateHandler.GenerateAuthorizeSignedCertificate(subject, "CN=" + CA_SUBJECT_NAME, caPrivateKey);
                if (newCertificate != null)
                {
                    activeCertificates.Add(newCertificate);
                    clientDict.Add(subject, address);
                }
            }

            retVal = new CertificateDto(newCertificate);
            return retVal;
        }

        /// <summary>
        /// Withdraw certificate and put them in CRL(Certificate Revocation List).
        /// Certificate can be withdrawn if his private key is compromited for example.
        /// </summary>
        /// <param name="certificate">Certificate</param>
        /// <returns></returns>
        public bool WithdrawCertificate(string subjectName)
        {
            bool isCertificateWithdrawn = false;
            X509Certificate2 activeCer = null;

            subjectName = "CN=" + subjectName;
            foreach(var item in activeCertificates)
            {
                if(item.SubjectName.Name.Equals(subjectName))
                {
                    activeCer = item;
                    break;
                }
            }

            if (activeCer != null)
            {
                activeCertificates.Remove(activeCer);
                if (!IsCertificateInCollection(activeCer, revocationList))
                {
                    revocationList.Add(activeCer);
                    isCertificateWithdrawn = true;
                }

            }

            return isCertificateWithdrawn;
        }

        /// <summary>
        /// Check if specified certificate is active.
        /// Active certificate is issued by CA and doesn't belong to CRL(Certificate Revocation List)
        /// </summary>
        /// <param name="certificate">Specified certificate</param>
        /// <returns></returns>
        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            bool isCertificateActive = false;

            if(!IsCertificateInCollection(certificate, revocationList))
            {
                if(IsCertificateInCollection(certificate, activeCertificates))
                {
                    isCertificateActive = true;
                }
            }

            return isCertificateActive;
        }

        public bool SaveCertificateToBackupDisc(CertificateDto certDto)
        {
            X509Certificate2 certificate = certDto.GetCert();
            activeCertificates.Add(certificate);
            CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, certificate, certificate.SubjectName.Name);

            return true;
        }

        public CAModelDto GetModel()
        {
            //TODO: implementirati getModel na CA servisu 
            CAModelDto retVal = new CAModelDto();

            retVal.ActiveCertificates = new HashSet<CertificateDto>();
            foreach(var cer in activeCertificates)
            {
                retVal.ActiveCertificates.Add(new CertificateDto(cer));
            }

            retVal.RevocationList = new HashSet<CertificateDto>();
            foreach(var cer in revocationList)
            {
                retVal.RevocationList.Add(new CertificateDto(cer));
            }

            retVal.ClientDict = new Dictionary<string, string>();
            foreach(var pair in clientDict)
            {
                retVal.ClientDict.Add(pair.Key, pair.Value);
            }

            retVal.CaCertificate = new CertificateDto(caCertificate);

            return retVal;
        }

        public bool SetModel(CAModelDto param)
        {
            bool retVal = true;
            
            if (File.Exists(PFX_PATH))
            {
                System.IO.File.Delete(PFX_PATH);
            }

            CertificateHandler.ReplaceCACertificateInStore(caCertificate, param.CaCertificate.GetCert());
            caCertificate = param.CaCertificate.GetCert();
            caPrivateKey = DotNetUtilities.GetKeyPair(caCertificate.PrivateKey).Private;
            CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, caCertificate, caCertificate.SubjectName.Name);

            activeCertificates.Clear();
            foreach(var cerDto in param.ActiveCertificates)
            {
                X509Certificate2 cert = cerDto.GetCert();
                activeCertificates.Add(cert);
                CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, cert, cert.SubjectName.Name);

                string fileName = cert.SubjectName.Name.Contains("=") ? cert.SubjectName.Name.Split('=')[1] : cert.SubjectName.Name;
                if (!File.Exists(CERT_FOLDER_PATH + cert.SubjectName.Name + fileName + ".pfx"))
                {
                    CertificateHandler.AddCertificateToStore(cert, StoreName.TrustedPeople, StoreLocation.LocalMachine);
                }
            }

            revocationList.Clear();
            foreach (var cerDto in param.RevocationList)
            {
                X509Certificate2 cert = cerDto.GetCert();
                revocationList.Add(cert);
            }

            clientDict.Clear();
            foreach (var pair in param.ClientDict)
            {
                clientDict.Add(pair.Key, pair.Value);
            }

            return retVal;
        }

        #endregion 

        #region Private methods

        /// <summary>
        /// Prepare certification authority service for use.
        /// Load information about CA.
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

        /// <summary>
        /// Check does CA contains certificate with specified subject name.
        /// </summary>
        /// <param name="subjectName">Subject name</param>
        /// <returns></returns>
        private X509Certificate2 IsCertificatePublished(string subjectName)
        {
            X509Certificate2 certificate = null;

            foreach(var cer in activeCertificates)
            {
                if(cer.SubjectName.Name.Equals("CN=" + subjectName))
                {
                    certificate = cer;
                    break;
                }
            }

            return certificate;
        }

        /// <summary>
        /// Check does certificate belong to specifed collection
        /// </summary>
        /// <param name="cer">Certificate</param>
        /// <param name="collection">Collection</param>
        /// <returns></returns>
        private bool IsCertificateInCollection(X509Certificate2 cer, HashSet<X509Certificate2> collection)
        {
            bool isCertificateInCollection = false;

            foreach (var item in collection)
            {
                if(item.Issuer.Equals(cer.Issuer) && item.SubjectName.Name.Equals(cer.SubjectName.Name) && item.Thumbprint.Equals(cer.Thumbprint) 
                    && item.SignatureAlgorithm.Value.Equals(cer.SignatureAlgorithm.Value) && item.SerialNumber.Equals(cer.SerialNumber) 
                    && item.PublicKey.ToString().Equals(cer.PublicKey.ToString()) && item.NotAfter.ToLongDateString().Equals(cer.NotAfter.ToLongDateString())
                    && item.NotBefore.ToLongDateString().Equals(cer.NotBefore.ToLongDateString()))
                {
                    isCertificateInCollection = true;
                    break;
                }
            }


            return isCertificateInCollection;
        }

        #endregion 

    }
}
