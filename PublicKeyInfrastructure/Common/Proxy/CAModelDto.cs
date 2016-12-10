using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Proxy
{
    public class CAModelDto
    {
        #region Fields

        private HashSet<CertificateDto> activeCertificates;
        private HashSet<CertificateDto> revocationList;
        private Dictionary<string, string> clientDict;

        #endregion

        #region Properties

        public HashSet<CertificateDto> ActiveCertificates
        {
            get
            {
                return activeCertificates;
            }
            set
            {
                activeCertificates = value;
            }
        }

        public HashSet<CertificateDto> RevocationList
        {
            get
            {
                return revocationList;
            }
            set
            {
                revocationList = value;
            }
        }

        public Dictionary<string, string> ClientDict
        {
            get
            {
                return clientDict;
            }
            set
            {
                clientDict = value;
            }
        }

        #endregion

    }
}
