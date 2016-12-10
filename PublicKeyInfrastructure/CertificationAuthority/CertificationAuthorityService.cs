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

            return retVal;
        }

        public bool SetModel(CAModelDto param)
        {
            //TODO: implementirati setModel na CA servisu
            bool retVal = false;

            activeCertificates.Clear();
            //mozda i obrisati postojece sertifikate u folderu ili racunati da ce oni biti pregazeni novim fajlovima
            foreach(var cerDto in param.ActiveCertificates)
            {
                X509Certificate2 cert = cerDto.GetCert();
                activeCertificates.Add(cert);
                CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, cert, cert.SubjectName.Name);
            }

            revocationList.Clear();
            foreach (var cerDto in param.RevocationList)
            {
                //mozda ubaciti i brisanje postojeceg sertifikata iz foldera
                X509Certificate2 cert = cerDto.GetCert();
                revocationList.Add(cert);
            }

            clientDict.Clear();
            foreach (var pair in param.ClientDict)
            {
                clientDict.Add(pair.Key, pair.Value);
            }

            retVal = true;
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

        #endregion 

    }
}
