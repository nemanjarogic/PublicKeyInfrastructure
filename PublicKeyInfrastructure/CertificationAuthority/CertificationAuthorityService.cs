using Common.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly string makecertDirectory;

        #endregion

        #region Constructor

        public CertificationAuthorityService()
        {
            activeCertificates = new HashSet<X509Certificate2>();
            revocationList = new HashSet<X509Certificate2>();

            makecertDirectory = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin";
        }

        #endregion

        #region Public methods

        public X509Certificate2 GenerateCertificate(string subjectName)
        {
            X509Certificate2 certificate = null;

            GenerateCertificateFromTerminal(subjectName);

            return certificate;
        }

        public bool WithdrawCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        public bool IsCertificateActive(X509Certificate2 certificate)
        {
            return true;
        }

        #endregion 

        #region Private methods

        private void GenerateCertificateFromTerminal(string subjectName)
        {
            string command = "makecert -n \"CN= " + subjectName + "\" -r -sv " + subjectName + ".pvk " + subjectName + ".cer";
            bool isCommandExecuted = TerminalManager.ExecuteTerminalCommand(makecertDirectory, command);
        }

        #endregion 
    }
}
