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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CertificationAuthority
{
    public class CertificateManager
    {
        private static void create()
        {
            try
            {
                AsymmetricKeyParameter caPrivateKey = null;
                //generate a root CA cert and obtain the privateKey
                X509Certificate2 MyRootCAcert = GenerateCACertificate("CN=RAJ CA 2014", ref caPrivateKey);
                AddCertificateToStore(MyRootCAcert, StoreName.Root, StoreLocation.LocalMachine);

                //generate cert based on the CA cert privateKey
                X509Certificate2 MyCert = GenerateSelfSignedCertificate("CN=Raj Kumar", "CN=RAJ CA 2014", caPrivateKey);
                AddCertificateToStore(MyCert, StoreName.My, StoreLocation.LocalMachine);

                //Export as pfx with privatekey
                byte[] certData = MyRootCAcert.Export(X509ContentType.Pfx, "123");
                File.WriteAllBytes(@"D:\raj CA.pfx", certData);

                //Export as pfx with privatekey
                certData = MyCert.Export(X509ContentType.Pfx, "123");
                File.WriteAllBytes(@"D:\raj kumar.pfx", certData);

                Console.WriteLine("CA:" + MyRootCAcert.Subject);
                Console.WriteLine("User cert:" + MyCert.Subject);

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }

        public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName, AsymmetricKeyParameter issuerPrivKey)
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

            // Generating the Certificate
            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;

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

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
            return x509;

        }

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

            // selfsign certificate
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(issuerKeyPair.Private, random);
            X509Certificate2 x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate.GetEncoded());

            refCaPrivateKey = issuerKeyPair.Private;

            AddCertificateToStore(x509, StoreName.Root, StoreLocation.LocalMachine);
            byte[] certData = x509.Export(X509ContentType.Pfx, "123");

            string fileName = String.Empty;
            if(subjectName.Contains(":"))
            {
                fileName = subjectName.Split(':')[1] + ".pfx";
            }
            else
            {
                fileName = subjectName.Trim() + ".pfx";;
            }
            File.WriteAllBytes(@"..\..\SecurityStore\" + fileName, certData);

            return x509;
        }

        public static bool AddCertificateToStore(X509Certificate2 cert, StoreName st, StoreLocation sl)
        {
            bool bRet = false;

            try
            {
                X509Store store = new X509Store(st, sl);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception on adding certificate to store. Message: " + ex.Message);
            }

            return bRet;
        }
    }
}
