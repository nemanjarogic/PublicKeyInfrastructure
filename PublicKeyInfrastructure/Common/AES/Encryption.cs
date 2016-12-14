using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.AES
{
    /// <summary>
    /// Extends AESOperations, and adds method for encryption one data block
    /// </summary>
    public class Encryption : AESOperations
    {
        public Encryption() { }

        /// <summary>
        /// Encryption one data block
        /// </summary>
        /// <param name="dataBlock">Byte matrix which for encryption</param>
        /// <param name="roundKeys"></param>
        /// <returns>Encrypted data</returns>
        public byte[] EncryptBlock(ref byte[][] dataBlock, List<byte[][]> roundKeys)
        {
            byte[] cipherBlock = new byte[16];

            AddRoundKey(dataBlock, roundKeys[0]);

            for (int i = 1; i < 11; i++)
            {
                SubBytes(dataBlock);
                ShiftRows(dataBlock);
                if (i != 10)
                {
                    MixColumns(ref dataBlock);
                }

                AddRoundKey(dataBlock, roundKeys[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cipherBlock[4 * i + j] = dataBlock[j][i];
                }
            }
            return cipherBlock;
        }
    }
}
