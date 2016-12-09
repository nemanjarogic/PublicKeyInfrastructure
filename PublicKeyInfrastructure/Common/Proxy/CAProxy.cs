using Common.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Proxy
{
    public class CAProxy : ChannelFactory<ICertificationAuthorityContract>, IDisposable
    {
        #region Fields

        private ICertificationAuthorityContract factory;

        private static string addressOfHotCAHost = null;
        private static string addressOfBackupCAHost = null;
        private static NetTcpBinding binding = null;
        private static string CERT_FOLDER_PATH = @"..\..\SecurityStore\";

        private enum EnumCAServerState { BothOn = 0, OnlyActiveOn = 1, BothOff = 2 };
        private static EnumCAServerState CA_SERVER_STATE = EnumCAServerState.BothOn;
        private static string ACTIVE_SERVER_ADDRESS = null;
        private static string NON_ACTIVE_SERVER_ADDRESS = null;

        static CAProxy()
        {
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            addressOfHotCAHost = "net.tcp://localhost:10000/CertificationAuthority";
            addressOfBackupCAHost = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";
            ACTIVE_SERVER_ADDRESS = addressOfHotCAHost;
            NON_ACTIVE_SERVER_ADDRESS = addressOfBackupCAHost;
        }

        #endregion

        #region Constructor

        private CAProxy(NetTcpBinding binding, string address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        #endregion

        #region Private methods

        private static void switchActiveNonActiveAddress()
        {
            string temp = NON_ACTIVE_SERVER_ADDRESS;
            NON_ACTIVE_SERVER_ADDRESS = ACTIVE_SERVER_ADDRESS;
            ACTIVE_SERVER_ADDRESS = temp;
        }

        private static bool IsCertificateActive_HotBackup(X509Certificate2 certificate)
        {
            bool retValue = false;

            try
            {
                //try communication with ACTIVE CA server
                using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                {
                    retValue = activeProxy.factory.IsCertificateActive(certificate);
                }
            }
            catch (EndpointNotFoundException exACTIVE)
            {
                try
                {
                    //try communication with NONACTIVE CA server
                    using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                    {
                        retValue = nonActiveProxy.factory.IsCertificateActive(certificate);

                        switchActiveNonActiveAddress();
                        CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                    }
                }
                catch (EndpointNotFoundException exNONACTIVE)
                {
                    Console.WriteLine("Both of CA servers not working!");
                    CA_SERVER_STATE = EnumCAServerState.BothOff;
                    return retValue;
                }

            }

            return retValue;
        }

        private static X509Certificate2 GenerateCertificate_HotBackup(string address)
        {
            X509Certificate2 certificate = null;

            try
            {
                //try communication with ACTIVE CA server
                using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                {
                    certificate = activeProxy.factory.GenerateCertificate(address);
                    if (certificate != null)
                    {
                        FileStream certFileStream = activeProxy.factory.GetFileStreamOfCertificate(address);
                        //TODO: obavezno pogledati kada zatvoriti ovaj filestream (na CAProxy-u ili na CAService-u)!!!!

                        #region try replication to NONACTIVE CA server
                        try
                        {
                            //replicate to NONACTIVE server
                            using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                            {
                                if (CA_SERVER_STATE == EnumCAServerState.BothOn)
                                {
                                    nonActiveProxy.factory.SaveCertificateToBackupDisc(certificate, certFileStream, address);
                                    //mozda ovde zatvoriti file stream
                                }
                                else if (CA_SERVER_STATE == EnumCAServerState.OnlyActiveOn)
                                {
                                    //nonActiveProxy.factory.INTEGRITY_UPDATE!!!
                                    IntegrityUpdate(activeProxy, nonActiveProxy);
                                    CA_SERVER_STATE = EnumCAServerState.BothOn;
                                }
                            }
                        }
                        catch (EndpointNotFoundException exNONACTIVE)
                        {
                            CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                        }
                        #endregion
                    }
                }
            }
            catch (EndpointNotFoundException exACTIVE)
            {
                try
                {
                    //try communication with NONACTIVE CA server
                    using (CAProxy backupProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                    {
                        certificate = backupProxy.factory.GenerateCertificate(address);

                        switchActiveNonActiveAddress();
                        CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                    }
                }
                catch (EndpointNotFoundException exNONACTIVE)
                {
                    Console.WriteLine("Both of CA servers not working!");
                    CA_SERVER_STATE = EnumCAServerState.BothOff;
                    return certificate;
                }

            }

            return certificate;
        }

        private static bool IntegrityUpdate(CAProxy activeProxy, CAProxy nonActiveProxy)
        {
            bool retVal = false;
            object objModel = null;

            //TODO: implementirati integrity update
            objModel = activeProxy.factory.GetModel();
            retVal = nonActiveProxy.factory.SetModel(objModel);

            /*try
            {
                using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                {
                    objModel = activeProxy.factory.GetModel();
                }
                using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                {
                    retVal = nonActiveProxy.factory.SetModel(objModel);
                }
            } 
            catch(EndpointNotFoundException exEndpoint)
            {
                Console.WriteLine("Integrity update failed!");
            }*/

            return retVal;
        }

        #endregion

        #region Public methods

        public static X509Certificate2 GenerateCertificate(string address)
        {
            X509Certificate2 certificate = null;

            certificate = GenerateCertificate_HotBackup(address);

            /*
            try
            {
                //try communication with HOT CA server
                using (CAProxy hotProxy = new CAProxy(binding, addressOfHotCAHost))
                {
                    certificate = hotProxy.factory.GenerateCertificate(subjectName);
                    try
                    {
                        if (certificate != null)
                        {
                            //replicate to backup server
                            using (CAProxy backupProxy = new CAProxy(binding, addressOfBackupCAHost))
                            {
                                backupProxy.factory.SaveCertificateToBackupDisc(certificate, hotProxy.factory.GetFileStreamOfCertificate(subjectName), subjectName);
                            }
                        }
                    }
                    catch (EndpointNotFoundException exBACKUP)
                    {

                    }
                }
            }
            catch (EndpointNotFoundException exHOT)
            {
                try
                {
                    //try communication with BACKUP CA server
                    using (CAProxy backupProxy = new CAProxy(binding, addressOfBackupCAHost))
                    {
                        certificate = backupProxy.factory.GenerateCertificate(subjectName);
                    }
                }
                catch (EndpointNotFoundException exBACKUP)
                {
                    Console.WriteLine("Both of CA servers not working!");
                    return certificate;
                }

            }
            */
            return certificate;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public static bool IsCertificateActive(X509Certificate2 certificate)
        {
            bool retValue = false;

            retValue = IsCertificateActive_HotBackup(certificate);

            /*
            try
            {
                //try communication with HOT CA server
                using (CAProxy hotProxy = new CAProxy(binding, addressOfHotCAHost))
                {
                    retValue = hotProxy.factory.IsCertificateActive(certificate);
                }
            }
            catch (EndpointNotFoundException exHOT)
            {
                try
                {
                    //try communication with BACKUP CA server
                    using (CAProxy backupProxy = new CAProxy(binding, addressOfBackupCAHost))
                    {
                        retValue = backupProxy.factory.IsCertificateActive(certificate);
                    }
                }
                catch (EndpointNotFoundException exBACKUP)
                {
                    Console.WriteLine("Both of CA servers not working!");
                    return retValue;
                }

            }
            */
            return retValue;
        }

        #endregion

        #region IDisposable methods

        public void Dispose()
        {
            if(factory != null)
            {
                factory = null;
            }

            this.Abort();   //*********************************** OBAVEZNO, INACE BACA CommunicationObjectFaultedException

            this.Close();
        }

        #endregion        
    }
}
