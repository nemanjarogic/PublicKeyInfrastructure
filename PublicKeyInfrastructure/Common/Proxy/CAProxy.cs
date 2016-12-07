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
    public class CAProxy : ChannelFactory<ICertificationAuthorityContract>, ICertificationAuthorityContract, IDisposable
    {
        #region Fields

        private ICertificationAuthorityContract factory;

        #endregion

        #region Constructor

        public CAProxy(NetTcpBinding binding, string address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        #endregion

        #region Public methods

        public X509Certificate2 GenerateCertificate(string subjectName)
        {
            X509Certificate2 certificate = null;
            certificate = factory.GenerateCertificate(subjectName);

            return certificate;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            return factory.IsCertificateActive(certificate);
        }

        #endregion

        #region IDisposable methods

        public void Dispose()
        {
            if(factory != null)
            {
                factory = null;
            }

            this.Close();
        }

        #endregion        
    }
}
