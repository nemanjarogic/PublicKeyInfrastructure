using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptography.AES
{
    /// <summary>
    /// Operations which is used in 10 iterations of AES algorithm
    /// </summary>
    public class AESOperations
    {
        /// <summary>
        /// Replacing elements in dataBlock with corresponding from SBOX table, or inverse SBOX table
        /// </summary>
        /// <param name="dataBlock">Matrix which elements need to be replaced</param>
        /// <param name="inverse">If true SBOX table is use, else inverse SBOX</param>
        protected void SubBytes(byte[][] dataBlock, bool inverse = false)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    UInt32 row = ((UInt32)dataBlock[i][j] & 0xf0) >> 4;
                    UInt32 column = (UInt32)dataBlock[i][j] & 0x0f;
                    dataBlock[i][j] = !inverse ? AES128_ECB.SBOX[row, column] : AES128_ECB.RSBOX[row, column];
                }
            }
        }

        /// <summary>
        /// Shifting rows in matrix
        /// </summary>
        /// <param name="dataBlock"></param>
        /// <param name="inverse"></param>
        protected void ShiftRows(byte[][] dataBlock, bool inverse = false)
        {
            for (int i = 0; i < 4; i++)
            {
                dataBlock[i] = ShiftRow(dataBlock[i], i, inverse);
            }
        }

        /// <summary>
        /// Shifting byte array for 0, 1, 2, or 3 positions, left or right
        /// </summary>
        /// <param name="row">Byte array for shifting</param>
        /// <param name="shiftSize"></param>
        /// <param name="inverse">If true shift left, else shift right</param>
        /// <returns></returns>
        private byte[] ShiftRow(byte[] row, int shiftSize, bool inverse)
        {
            byte[] retValue = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (!inverse)
                {
                    retValue[i - shiftSize < 0 ? 4 - shiftSize + i : i - shiftSize] = row[i];
                }
                else
                {
                    retValue[i + shiftSize > 3 ? shiftSize + i - 4 : i + shiftSize] = row[i];
                }
            }
            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBlock"></param>
        /// <param name="inverse"></param>
        protected void MixColumns(ref byte[][] dataBlock, bool inverse = false)
        {
            byte[][] mixedDataBlock = new byte[4][];
            for (int i = 0; i < 4; i++)
            {
                mixedDataBlock[i] = new byte[4];
            }
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    mixedDataBlock[i][j] = GetMixedValue(i, j, dataBlock, inverse);
                }
            }
            dataBlock = mixedDataBlock;
        }

        /// <summary>
        /// Galois Field (256) Multiplication of two Bytes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Byte GMul(byte a, byte b)
        {
            byte p = 0;
            byte counter;
            byte hi_bit_set;
            for (counter = 0; counter < 8; counter++)
            {
                if ((b & 1) != 0)
                {
                    p ^= a;
                }
                hi_bit_set = (byte)(a & 0x80);
                a <<= 1;
                if (hi_bit_set != 0)
                {
                    a ^= 0x1b; /* x^8 + x^4 + x^3 + x + 1 */
                }
                b >>= 1;
            }
            return p;
        }

        private byte GetMixedValue(int i, int j, byte[][] dataBlock, bool inverse)
        {
            byte retValue = 0x00;
            for (int k = 0; k < 4; k++)
            {
                retValue ^= GMul(!inverse ? AES128_ECB.MIX_COLUMN[i, k] : AES128_ECB.RMIX_COLUMN[i, k], dataBlock[k][j]);
            }
            return retValue;
        }

        /// <summary>
        /// Adding round key on dataBlock in one round
        /// </summary>
        /// <param name="dataBlock"></param>
        /// <param name="roundKey"></param>
        protected void AddRoundKey(byte[][] dataBlock, byte[][] roundKey)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    dataBlock[i][j] ^= roundKey[i][j];
                }
            }
        }
    }
}
