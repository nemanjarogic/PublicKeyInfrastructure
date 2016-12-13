using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.AES
{
    public class TestAES
    {
        private Random _random = new Random(Environment.TickCount);
        private static readonly int iterNum = 100;

        public void Test()
        {
            for (int i = 0; i < iterNum; i++)
            {
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine();

                string plainText = RandomString(11);
                string key = RandomString(16);

                Console.WriteLine("PlainText: {0}", plainText);

                AES128_ECB aes = new AES128_ECB(System.Text.Encoding.UTF8.GetBytes(key));
                byte[] cipher = aes.Encrypt(System.Text.Encoding.UTF8.GetBytes(plainText));

                Console.WriteLine("Encrypted: {0}", System.Text.Encoding.UTF8.GetString(cipher));

                byte[] cipherProfi = AES_Encrypt(aes.PrepareData(System.Text.Encoding.UTF8.GetBytes(plainText)), System.Text.Encoding.UTF8.GetBytes(key));

                Console.WriteLine("Encrypted: {0}", System.Text.Encoding.UTF8.GetString(cipherProfi));

                //string enc = System.Text.Encoding.UTF8.GetString(AES_Decrypt(cipher, System.Text.Encoding.UTF8.GetBytes(key)));
                string enc = System.Text.Encoding.UTF8.GetString(aes.Decrypt(cipherProfi));
                if (!enc.Trim().Equals(plainText))
                {
                    Console.WriteLine("Error!!! {0}", enc);
                    Console.ReadLine();
                }
                Console.WriteLine("Decrypted: {0}", enc);
            }
        }

        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 128;
                    AES.BlockSize = 128;
                    AES.Padding = PaddingMode.None;
                    // var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = passwordBytes;//key.GetBytes(AES.KeySize / 8);
                                            // AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.ECB;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Padding = PaddingMode.None;
                    //var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = passwordBytes;//key.GetBytes(AES.KeySize / 8);
                    //AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.ECB;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public string RandomString(int length)
        {
            string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < length; ++i)
                builder.Append(chars[_random.Next(chars.Length)]);

            return builder.ToString();
        }
    }
}
