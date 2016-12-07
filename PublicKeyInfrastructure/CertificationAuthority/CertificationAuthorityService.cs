using Common.Server;
using System;
using System.Collections.Generic;
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

        #endregion

        #region Properties

        public HashSet<X509Certificate2> ActiveCertificates
        {
            get
            {
                return activeCertificates;
            }
        }

        public HashSet<X509Certificate2> RevocationList
        {
            get
            {
                return revocationList;
            }
        }

        #endregion

        #region Public methods

        public X509Certificate2 GenerateCertificate(string subjectName)
        {
            throw new NotImplementedException();
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        #endregion 
    }
}
