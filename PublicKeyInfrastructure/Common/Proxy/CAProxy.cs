using Client;
using Common.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
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

        private enum EnumCAServerState { BothOn = 0, OnlyActiveOn = 1, BothOff = 2 };
        private static EnumCAServerState CA_SERVER_STATE = EnumCAServerState.BothOn;
        private static string ACTIVE_SERVER_ADDRESS = null;
        private static string NON_ACTIVE_SERVER_ADDRESS = null;

        private static object objLock = new object();

        #endregion

        #region Constructor

        private CAProxy(NetTcpBinding binding, string address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        static CAProxy()
        {
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            addressOfHotCAHost = "net.tcp://localhost:10000/CertificationAuthority";
            addressOfBackupCAHost = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";

            //addressOfHotCAHost = "net.tcp://10.1.212.118:10000/CertificationAuthority";
            //addressOfBackupCAHost = "net.tcp://localhost:10000/CertificationAuthority";

            ACTIVE_SERVER_ADDRESS = addressOfHotCAHost;
            NON_ACTIVE_SERVER_ADDRESS = addressOfBackupCAHost;

            Task task1 = Task.Factory.StartNew(() => TryIntegrityUpdate());
        }

        #endregion

        #region Public methods

        public static CertificateDto GenerateCertificate(string subject, string address)
        {
            CertificateDto retCertDto = null;
            X509Certificate2 certificate = null;

            lock (objLock)
            {

                try
                {
                    //try communication with ACTIVE CA server
                    using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                    {
                        retCertDto = activeProxy.factory.GenerateCertificate(subject, address);
                        certificate = retCertDto.GetCert();
                        if (certificate != null)
                        {
                            #region try replication to NONACTIVE CA server
                            try
                            {
                                //replicate to NONACTIVE server
                                using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                                {
                                    if (CA_SERVER_STATE == EnumCAServerState.BothOn)
                                    {
                                        nonActiveProxy.factory.SaveCertificateToBackupDisc(new CertificateDto(certificate));
                                    }
                                    else if (CA_SERVER_STATE == EnumCAServerState.OnlyActiveOn)
                                    {
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
                            retCertDto = backupProxy.factory.GenerateCertificate(subject, address);
                            certificate = retCertDto.GetCert();

                            SwitchActiveNonActiveAddress();
                            CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                        }
                    }
                    catch (EndpointNotFoundException exNONACTIVE)
                    {
                        Console.WriteLine("Both of CA servers not working!");
                        CA_SERVER_STATE = EnumCAServerState.BothOff;
                    }
                }
            }

            return retCertDto;
        }

        public static bool WithdrawCertificate(string subjectName)
        {
            string clientAddress = null;

            lock(objLock)
            {
                try
                {
                    //try communication with ACTIVE CA server
                    using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                    {
                        clientAddress = activeProxy.factory.WithdrawCertificate(subjectName);
                    }
                }
                catch (EndpointNotFoundException exACTIVE)
                {
                    try
                    {
                        //try communication with NONACTIVE CA server
                        using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                        {
                            clientAddress = nonActiveProxy.factory.WithdrawCertificate(subjectName);

                            SwitchActiveNonActiveAddress();
                            CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                        }
                    }
                    catch (EndpointNotFoundException exNONACTIVE)
                    {
                        Console.WriteLine("Both of CA servers not working!");
                        CA_SERVER_STATE = EnumCAServerState.BothOff;
                    }

                }

                if(clientAddress != null)
                {
                    NotifyClientsAboutCertificateWithdraw(clientAddress);
                }
            }

            return clientAddress != null;
        }

        public static bool IsCertificateActive(X509Certificate2 certificate)
        {
            bool retValue = false;

            lock (objLock)
            {
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

                            SwitchActiveNonActiveAddress();
                            CA_SERVER_STATE = EnumCAServerState.OnlyActiveOn;
                        }
                    }
                    catch (EndpointNotFoundException exNONACTIVE)
                    {
                        Console.WriteLine("Both of CA servers not working!");
                        CA_SERVER_STATE = EnumCAServerState.BothOff;
                    }

                }
            }

            return retValue;
        }

        #endregion


        #region Private methods

        private static void SwitchActiveNonActiveAddress()
        {
            string temp = NON_ACTIVE_SERVER_ADDRESS;
            NON_ACTIVE_SERVER_ADDRESS = ACTIVE_SERVER_ADDRESS;
            ACTIVE_SERVER_ADDRESS = temp;
        }

        private static void TryIntegrityUpdate()
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (CA_SERVER_STATE == EnumCAServerState.OnlyActiveOn)
                {
                    lock (objLock)
                    {

                        try
                        {
                            using (CAProxy activeProxy = new CAProxy(binding, ACTIVE_SERVER_ADDRESS))
                            {
                                using (CAProxy nonActiveProxy = new CAProxy(binding, NON_ACTIVE_SERVER_ADDRESS))
                                {
                                    //Task task1 = Task.Factory.StartNew(() => IntegrityUpdate(activeProxy, nonActiveProxy));
                                    IntegrityUpdate(activeProxy, nonActiveProxy);
                                }
                            }
                        }
                        catch (EndpointNotFoundException exEndpoint)
                        {

                        }
                    }
                }
            }
        }

        private static bool IntegrityUpdate(CAProxy activeProxy, CAProxy nonActiveProxy)
        {
            bool retVal = false;
            CAModelDto objModel = null;

            objModel = activeProxy.factory.GetModel();
            retVal = nonActiveProxy.factory.SetModel(objModel);

            return retVal;
        }

        /// <summary>
        /// When client certificate is withdrawn notify him about that.
        /// </summary>
        /// <param name="clientAddress">Client address</param>
        private static void NotifyClientsAboutCertificateWithdraw(string clientAddress)
        {
            try
            {
                using (ClientProxy proxy = new ClientProxy(new EndpointAddress(clientAddress), new NetTcpBinding(), null))
                {
                    proxy.RemoveInvalidClient(null);
                }
            }
            catch(Exception ex)
            {
                string logMessage = "Client not found on withdraw certificate action. Specified client address is '" + clientAddress + "'. Exception message: " + ex.Message;
                Audit.WriteEvent(logMessage, EventLogEntryType.FailureAudit);
            }
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
