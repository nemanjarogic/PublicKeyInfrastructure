using Common.Server;
using System;
using System.Collections.Generic;
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

        static CAProxy()
        {
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            addressOfHotCAHost = "net.tcp://localhost:10000/CertificationAuthority";
            addressOfBackupCAHost = "net.tcp://localhost:10001/CertificationAuthorityBACKUP";
        }

        #endregion

        #region Constructor

        private CAProxy(NetTcpBinding binding, string address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        #endregion

        #region Public methods

        public static X509Certificate2 GenerateCertificate(string subjectName)
        {
            X509Certificate2 certificate = null;

            try
            {
                //try communication with HOT CA server
                using (CAProxy hotProxy = new CAProxy(binding, addressOfHotCAHost))
                {
                    certificate = hotProxy.factory.GenerateCertificate(subjectName);
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

            return certificate;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public static bool IsCertificateActive(X509Certificate2 certificate)
        {
            bool retValue = false;

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
