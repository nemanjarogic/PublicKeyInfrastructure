using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.AES
{
    public class Decryption : AESOperations
    {
        public Decryption() { }

        public byte[] DecryptBlock(ref byte[][] cipherBlock, List<byte[][]> roundKeys)
        {
            byte[] data = new byte[16];

            AddRoundKey(cipherBlock, roundKeys[10]);

            for (int i = 9; i >= 0; i--)
            {
                SubBytes(cipherBlock, inverse: true);
                ShiftRows(cipherBlock, inverse: true);
                AddRoundKey(cipherBlock, roundKeys[i]);

                if (i != 0)
                {
                    MixColumns(ref cipherBlock, inverse: true);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    data[4 * i + j] = cipherBlock[j][i];
                }
            }
            return data;
        }
    }
}
