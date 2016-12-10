using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    /// <summary>
    /// CertificateHandler class is used for generating CA and authorize signed certificates
    /// This class also handle adding of certificate to store(installation) and export to file system
    /// </summary>
    public static class CertificateHandler
    {
        #region Public methods

        /// <summary>
        /// Generate, install and and .pfx(which represent CA) to file system
        /// </summary>
        /// <param name="subjectName">Subject name for CA(Certification authority)</param>
        /// <param name="refCaPrivateKey">Private key for generated CA</param>
        /// <returns></returns>
        public static X509Certificate2 GenerateCACertificate(string subjectName, ref AsymmetricKeyParameter refCaPrivateKey)
        {
            const int keyStrength = 2048;

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Signature Algorithm
            const string signatureAlgorithm = "SHA256WithRSA";
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

            // Issuer and Subject Name
            X509Name subjectDN = new X509Name(subjectName);
            X509Name issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(2);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;

            // Self-sign certificate
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(issuerKeyPair.Private, random);
            X509Certificate2 x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded(), "123", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

            RSA rsaPriv = DotNetUtilities.ToRSA(issuerKeyPair.Private as RsaPrivateCrtKeyParameters);
            x509.PrivateKey = rsaPriv;
            refCaPrivateKey = issuerKeyPair.Private;

            // Install certificate
            AddCertificateToStore(x509, StoreName.Root, StoreLocation.LocalMachine);

            // Export certificate and private key to PFX file
            ExportToFileSystem(X509ContentType.Pfx, x509, subjectName);

            return x509;
        }

        /// <summary>
        /// Generate, install and export to file system new certificate
        /// </summary>
        /// <param name="subjectName">Subject name for new certificate</param>
        /// <param name="issuerName">CA(Certificate authority) name</param>
        /// <param name="issuerPrivKey">Issuer private key</param>
        /// <returns></returns>
        public static X509Certificate2 GenerateAuthorizeSignedCertificate(string subjectName, string issuerName, AsymmetricKeyParameter issuerPrivKey)
        {
            const int keyStrength = 2048;

            // Generating random numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Signature Algorithm
            const string signatureAlgorithm = "SHA256WithRSA";
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

            // Issuer and Subject Name
            X509Name subjectDN = new X509Name("CN=" + subjectName);
            X509Name issuerDN = new X509Name(issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(2);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // selfsign certificate
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(issuerPrivKey, random);

            // correcponding private key
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);

            // merge into X509Certificate2
            X509Certificate2 x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            Asn1Sequence seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
            RsaPrivateKeyStructure rsa =  RsaPrivateKeyStructure.GetInstance(seq);
            RsaPrivateCrtKeyParameters rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

            // Set Private Key
            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);

            // Install certificate
            AddCertificateToStore(x509, StoreName.TrustedPeople, StoreLocation.LocalMachine);

            //Export
            ExportToFileSystem(X509ContentType.Pfx, x509, subjectName);

            return x509;
        }

        /// <summary>
        /// Convert AsymmetricAlgorithm private key type to BouncyCastle private key
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static AsymmetricKeyParameter TransformRSAPrivateKey(AsymmetricAlgorithm privateKey)
        {
            RSACryptoServiceProvider prov = privateKey as RSACryptoServiceProvider;
            RSAParameters parameters = prov.ExportParameters(true);

            return new RsaPrivateCrtKeyParameters(
                new BigInteger(1, parameters.Modulus),
                new BigInteger(1, parameters.Exponent),
                new BigInteger(1, parameters.D),
                new BigInteger(1, parameters.P),
                new BigInteger(1, parameters.Q),
                new BigInteger(1, parameters.DP),
                new BigInteger(1, parameters.DQ),
                new BigInteger(1, parameters.InverseQ));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Add certificate to store.
        /// Installation of the certificate.
        /// </summary>
        /// <param name="cert">Certificate to install</param>
        /// <param name="st">Store name</param>
        /// <param name="sl">Store location</param>
        /// <returns></returns>
        private static bool AddCertificateToStore(X509Certificate2 cert, StoreName st, StoreLocation sl)
        {
            bool isCertificateAdded = false;

            try
            {
                X509Store store = new X509Store(st, sl);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
                isCertificateAdded = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception on adding certificate to store. Message: " + ex.Message);
            }

            return isCertificateAdded;
        }

        /// <summary>
        /// Export desired content to file system.
        /// </summary>
        /// <param name="contentType">Content type can be PFX or CERT</param>
        /// <param name="certificate">Certificate to export</param>
        /// <param name="subjectName">Subject name(name of file on system)</param>
        /// <returns></returns>
        public static bool ExportToFileSystem(X509ContentType contentType, X509Certificate2 certificate , string subjectName)
        {
            bool isValidType = false;
            bool isExportDone = false;
            byte[] certData = null;

            if(contentType == X509ContentType.Pfx)
            {
                certData = certificate.Export(X509ContentType.Pfx, "123");
                isValidType = true;
            }
            else if(contentType == X509ContentType.Cert)
            {
                certData = certificate.Export(X509ContentType.Cert, "123");
                isValidType = true;
            }

            if(isValidType)
            {
                string fileName = String.Empty;
                string fileExtension = contentType == X509ContentType.Pfx ? ".pfx" : ".cer";

                if (subjectName.Contains("\\"))
                {
                    fileName = subjectName.Replace('\\','_') + fileExtension;
                }
                else
                {
                    fileName = subjectName.Trim() + fileExtension;
                }

                File.WriteAllBytes(@"..\..\SecurityStore\" + fileName, certData);
                isExportDone = true;
            }

            return isExportDone;
        }

        #endregion
    }
}
