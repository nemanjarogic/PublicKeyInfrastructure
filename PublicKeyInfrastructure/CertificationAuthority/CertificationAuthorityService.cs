using Common;
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
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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

            SetAccessControlList();
            PrepareCAService();
            Audit.WriteEvent("CertificationAuthorityService initialized.", EventLogEntryType.Information);
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
            if(!IsUserAccessGranted(WindowsIdentity.GetCurrent().Name))
            {
                Audit.WriteEvent("User '" + WindowsIdentity.GetCurrent().Name + "' had denied access for method GenerateCertificate", EventLogEntryType.FailureAudit);
            }

            CertificateDto retVal = null;
            X509Certificate2 newCertificate = null;
            string logMessage = String.Empty;

            newCertificate = IsCertificatePublished(subject);
            if (newCertificate == null)
            {
                newCertificate = CertificateHandler.GenerateAuthorizeSignedCertificate(subject, "CN=" + CA_SUBJECT_NAME, caPrivateKey);
                if (newCertificate != null)
                {
                    activeCertificates.Add(newCertificate);
                    clientDict.Add(subject, address);

                    logMessage = "Certificate with subject name '" + subject + "' is issued by '" + CA_SUBJECT_NAME + "'";
                    Audit.WriteEvent(logMessage, EventLogEntryType.Information);
                }
                else
                {
                    logMessage = "Generation of certificate with subject name '" + subject + "' failed.";
                    Audit.WriteEvent(logMessage, EventLogEntryType.Warning);
                }
            }
            else
            {
                logMessage = "Certificate with subject name '" + subject + "' is already published";
                Audit.WriteEvent(logMessage, EventLogEntryType.Warning);
            }

            retVal = new CertificateDto(newCertificate);
            return retVal;
        }

        /// <summary>
        /// Withdraw certificate and put them in CRL(Certificate Revocation List).
        /// Certificate can be withdrawn if his private key is compromited for example.
        /// </summary>
        /// <param name="certificate">Certificate</param>
        /// <returns>Endpoint address of client which certificate is withdrawn.</returns>
        public string WithdrawCertificate(string subjectName)
        {
            if (!IsUserAccessGranted(WindowsIdentity.GetCurrent().Name))
            {
                Audit.WriteEvent("User '" + WindowsIdentity.GetCurrent().Name + "' had denied access for method WithdrawCertificate", EventLogEntryType.FailureAudit);
            }

            string clientAddress = null;
            X509Certificate2 activeCer = null;

            foreach(var item in activeCertificates)
            {
                if (item.SubjectName.Name.Equals("CN=" + subjectName))
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
                    clientAddress = clientDict[subjectName];

                    string logMessage = "Certificate with subject name '" + subjectName + "' is successfully revoked.";
                    Audit.WriteEvent(logMessage, EventLogEntryType.Information);
                }
            }
            else
            {
                string logMessage = "Revocaton of certificate with subject name '" + subjectName + "' failed. Specified certificate isn't active.";
                Audit.WriteEvent(logMessage, EventLogEntryType.Warning);
            }

            return clientAddress;
        }

        /// <summary>
        /// Check if specified certificate is active.
        /// Active certificate is issued by CA and doesn't belong to CRL(Certificate Revocation List)
        /// </summary>
        /// <param name="certificate">Specified certificate</param>
        /// <returns></returns>
        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            if (!IsUserAccessGranted(WindowsIdentity.GetCurrent().Name))
            {
                Audit.WriteEvent("User '" + WindowsIdentity.GetCurrent().Name + "' had denied access for method IsCertificateActive", EventLogEntryType.FailureAudit);
            }

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

        /// <summary>
        /// Save specified certificate to backup
        /// </summary>
        /// <param name="certDto">Certificate</param>
        /// <returns></returns>
        public bool SaveCertificateToBackupDisc(CertificateDto certDto)
        {
            if (!IsUserAccessGranted(WindowsIdentity.GetCurrent().Name))
            {
                Audit.WriteEvent("User '" + WindowsIdentity.GetCurrent().Name + "' had denied access for method SaveCertificateToBackupDisc", EventLogEntryType.FailureAudit);
            }

            X509Certificate2 certificate = certDto.GetCert();
            activeCertificates.Add(certificate);
            CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, certificate, certificate.SubjectName.Name);

            string logMessage = "Certificate with subject name '" + certificate.SubjectName.Name + "' is saved on backup server.'";
            Audit.WriteEvent(logMessage, EventLogEntryType.Information);

            return true;
        }

        /// <summary>
        /// Get active data from hot server. 
        /// This is neccessary on integrity update.
        /// </summary>
        /// <returns>Active data from hot server</returns>
        public CAModelDto GetModel()
        {
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

        /// <summary>
        /// This method is used for integrity update.
        /// Integrity update implies data copy from hot to backup server.
        /// </summary>
        /// <param name="param">Data from hot server</param>
        /// <returns></returns>
        public bool SetModel(CAModelDto param)
        {
            bool retVal = true;
            
            // Copy and install valid CA certificate to backup server
            if (File.Exists(PFX_PATH))
            {
                System.IO.File.Delete(PFX_PATH);
            }

            CertificateHandler.ReplaceCACertificateInStore(caCertificate, param.CaCertificate.GetCert());
            caCertificate = param.CaCertificate.GetCert();
            caPrivateKey = DotNetUtilities.GetKeyPair(caCertificate.PrivateKey).Private;
            CertificateHandler.ExportToFileSystem(X509ContentType.Pfx, caCertificate, caCertificate.SubjectName.Name);

            // Export and install active client certificates on backup server
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

            // Set active revocation list on backup server
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

        /// <summary>
        /// Find client with subject name in list of active clients and remove it.
        /// This method is used as reaction on closing active client.
        /// </summary>
        /// <param name="subject">Subject name</param>
        /// <returns></returns>
        public bool RemoveClientFromListOfActiveClients(string subject)
        {
            bool retValue = false;

            if (clientDict.Remove(subject))
                retValue = true;

            return retValue;
        }

        #endregion 

        #region Private methods

        /// <summary>
        /// Add current user to ACL(Access contol list) for SecurityStore folder
        /// </summary>
        private void SetAccessControlList()
        {
            try
            {
                var accessRule = new FileSystemAccessRule(
                                     WindowsIdentity.GetCurrent().Name,
                                     fileSystemRights: FileSystemRights.FullControl,
                                     inheritanceFlags: InheritanceFlags.None,
                                     propagationFlags: PropagationFlags.None,
                                     type: AccessControlType.Allow);

                var directoryInfo = new DirectoryInfo(CERT_FOLDER_PATH);

                // Get a DirectorySecurity object that represents the current security settings.
                DirectorySecurity dSecurity = directoryInfo.GetAccessControl();

                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(accessRule);

                // Set the new access settings.
                directoryInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                Audit.WriteEvent("Exception in SetAccessControlList(). Exception message: " + ex.Message, EventLogEntryType.FailureAudit);
            }
        }

        /// <summary>
        /// Prepare certification authority service for use.
        /// Load information about CA.
        /// </summary>
        private void PrepareCAService()
        {
            bool isPfxCreated = true;
            bool isCertFound = false;
            X509Certificate2Collection collection = new X509Certificate2Collection();
            
            if(!IsUserAccessGranted(WindowsIdentity.GetCurrent().Name))
            {
                Audit.WriteEvent("Access to SecurityStore is denied to user '" + WindowsIdentity.GetCurrent().Name + "' based on ACL content.", EventLogEntryType.Warning);
                return;
            }

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

            Audit.WriteEvent("Certificate for the CA is successfully loaded.", EventLogEntryType.Information);
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

        /// <summary>
        /// Check does specified user has authority for access to SecurityStore folder.
        /// </summary>
        /// <param name="user">User</param>
        /// <returns></returns>
        private bool IsUserAccessGranted(string user)
        {
            bool accessGranted = false;

            FileSecurity security = File.GetAccessControl(CERT_FOLDER_PATH);
            AuthorizationRuleCollection acl = security.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule ace in acl)
            {
                if (ace.IdentityReference.Value.Equals(user))
                {
                    accessGranted = true;
                    break;
                }
            }

            return accessGranted;
        }

        #endregion 

    }
}
