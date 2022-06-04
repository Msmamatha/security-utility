

using System;
using System.Text;
using System.IO;


namespace EncryptionLib
{
    class AES 
    {
        private byte[] Sbox, iSbox;
        private byte[] Round;

        protected byte[] Key = null;
        private byte[] Keys;
        private int Nr;
      
        public AES()
        {
            InitializeTables();
        }

        protected byte[] Encrypt128Bit(byte[] block)
        {
            AddRoundKey(block, 0);
            //Nr=10,12 or 14 depending on key size
            for (int i = 1; i < Nr; i++)
            {
                SubBytes(block);
                ShiftRows(block);
                MixColumns(block);
                AddRoundKey(block, i);
            }
            SubBytes(block);
            ShiftRows(block);
            AddRoundKey(block, Nr);
            return block;
        }

        protected byte[] Decrypt128Bit(byte[] block)
        {
            AddRoundKey(block, Nr);
            //Nr=10,12 or 14 depending on key size
            for (int i = Nr - 1; i > 0; i--)
            {
                InvShiftRows(block);
                InvSubBytes(block);
                AddRoundKey(block, i);
                InvMixColumns(block);
            }
            InvShiftRows(block);
            InvSubBytes(block);
            AddRoundKey(block, 0);
            return block;
        }
               
        private void SubBytes(byte[] block)
        {
            for (int i = 0; i < 16; i++)
            {
                block[i] = Sbox[block[i]];
            }
        }

        private void InvSubBytes(byte[] block)
        {
            for (int i = 0; i < 16; i++)
            {
                block[i] = iSbox[block[i]];
            }
        }
          
        void ShiftRows(byte[] state)
        {
            byte[] t = new byte[4];
            for (int r = 1; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                    t[c] = state[r * 4 + ((c + r) % 4)];
                for (int c = 0; c < 4; c++)
                    state[r * 4 + c] = t[c];
            }
        }

        void InvShiftRows(byte[] state)
        {
            byte[] t = new byte[4];
            for (int r = 1; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                    t[(c + r) % 4] = state[r * 4 + c];
                for (int c = 0; c < 4; c++)
                    state[r * 4 + c] = t[c];
            }
        }
        private void AddRoundKey(byte[] block, int round)
        {
            for (int i = 0; i < 16; i++)
            {
                block[i] = (byte)(block[i] ^ Keys[(round * 4 + i )]);
            }
        }
        
        private void MixColumns(byte[] block)
        {
            //temp=block
            byte[,] t = new byte[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    t[i, j] = block[i * 4 + j];
                }
            }
            for (int i = 0; i < 4; i++)
            {
                block[00 + i] = (byte)(M(2, t[0, i]) ^ M(3, t[1, i]) ^ t[2, i] ^ t[3, i]);
                block[04 + i] = (byte)(t[0, i] ^M(2, t[1, i])^ M(3, t[2, i]) ^  t[3, i] );
                block[08 + i] = (byte)(t[0, i] ^ t[1, i] ^ M(2, t[2, i]) ^ M(3, t[3, i]));
                block[12 + i] = (byte)(M(3, t[0, i]) ^ t[1, i] ^ t[2, i] ^ M(2, t[3, i]));
            }
        }

        private void InvMixColumns(byte[] block)
        {
            byte[,] t = new byte[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    t[i, j] = block[i * 4 + j];
                }
            }
            for (int i = 0; i < 4; i++)
            {
                block[00 + i] = (byte)(M(14, t[0, i]) ^ M(11, t[1, i]) ^ M(13, t[2, i]) ^ M(9, t[3, i]));
                block[04 + i] = (byte)(M(9, t[0, i]) ^ M(14, t[1, i]) ^ M(11, t[2, i]) ^ M(13, t[3, i]));
                block[08 + i] = (byte)(M(13, t[0, i]) ^ M(9, t[1, i]) ^ M(14, t[2, i]) ^ M(11, t[3, i]));
                block[12 + i] = (byte)(M(11, t[0, i]) ^ M(13, t[1, i]) ^ M(9, t[2, i]) ^ M(14, t[3, i]));
            }
        }
        
        
        private int M(int a, int b)
        {
            int aa = a, bb = b, r = 0, t;
            while (aa != 0)
            {
                if ((aa & 1) != 0)
                    r ^= bb;
                t = bb & 0x80;
                bb <<= 1;
                if (t != 0)
                    bb ^= 0x1b;
                aa >>= 1;
            }
            return r;
        }    
       
        protected void ExpandKey(string key)
        {
            if (!(key.Length == 16 || key.Length == 24 || key.Length == 32))
            {
                throw new Exception("Key Lenght Must Be 16 or 24 or 32 Characters Long..");
            }

            int Nk = key.Length / 4;
            Nr = Nk + 6;

            Key = new byte[key.Length * 8];
            Keys = new byte[4 * 4 * (Nr + 1)];
            Encoding.ASCII.GetBytes(key).CopyTo(Key, 0);

            for (int i = 0; i < Nk; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Keys[i*4+ j] = Key[4 * i + j];
                }
            }

            byte[] temp = new byte[4];
            for (int i = Nk; i < 4 * (Nr + 1); i++)
            {
                temp[0] = Keys[(i - 1) * 4 + 0];
                temp[1] = Keys[(i - 1) * 4 + 1];
                temp[2] = Keys[(i - 1) * 4 + 2];
                temp[3] = Keys[(i - 1) * 4 + 3];

                if (i % Nk == 0)
                {
                    temp = SubWord(RotWord(temp));
                    temp[0] = (byte)(temp[0] ^ Round[i / Nk]);
                }
                else if (Nk > 6 && (i % Nk == 4))
                {
                    temp = SubWord(temp);
                }

                Keys[i * 4 + 0] = (byte)(Keys[(i - Nk) * 4 + 0] ^ temp[0]);
                Keys[i * 4 + 1] = (byte)(Keys[(i - Nk) * 4 + 1] ^ temp[1]);
                Keys[i * 4 + 2] = (byte)(Keys[(i - Nk) * 4 + 2] ^ temp[2]);
                Keys[i * 4 + 3] = (byte)(Keys[(i - Nk) * 4 + 3] ^ temp[3]);

            }
        }

        private byte[] SubWord(byte[] temp)
        {
            return new byte[4] { 
                Sbox[temp[0]],
                Sbox[temp[1]],
                Sbox[temp[2]],
                Sbox[temp[3]]
            };
        }

        private byte[] RotWord(byte[] temp)
        {
            return new byte[4] { temp[1], temp[2], temp[3], temp[0] };
        }

        protected void InitializeTables()
        {
            Round = new byte[11] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36 };

            Sbox = new byte[16 * 16] 
            {    
                    0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76,
                    0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0,
                    0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15,
                    0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75,
                    0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84,
                    0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf,
                    0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8,
                    0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2,
                    0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73,
                    0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb,
                    0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79,
                    0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08,
                    0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a,
                    0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e,
                    0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf,
                    0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16 
            };

            iSbox = new byte[16 * 16]
            {     
                    0x52, 0x09, 0x6a, 0xd5, 0x30, 0x36, 0xa5, 0x38, 0xbf, 0x40, 0xa3, 0x9e, 0x81, 0xf3, 0xd7, 0xfb,
                    0x7c, 0xe3, 0x39, 0x82, 0x9b, 0x2f, 0xff, 0x87, 0x34, 0x8e, 0x43, 0x44, 0xc4, 0xde, 0xe9, 0xcb,
                    0x54, 0x7b, 0x94, 0x32, 0xa6, 0xc2, 0x23, 0x3d, 0xee, 0x4c, 0x95, 0x0b, 0x42, 0xfa, 0xc3, 0x4e,
                    0x08, 0x2e, 0xa1, 0x66, 0x28, 0xd9, 0x24, 0xb2, 0x76, 0x5b, 0xa2, 0x49, 0x6d, 0x8b, 0xd1, 0x25,
                    0x72, 0xf8, 0xf6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xd4, 0xa4, 0x5c, 0xcc, 0x5d, 0x65, 0xb6, 0x92,
                    0x6c, 0x70, 0x48, 0x50, 0xfd, 0xed, 0xb9, 0xda, 0x5e, 0x15, 0x46, 0x57, 0xa7, 0x8d, 0x9d, 0x84,
                    0x90, 0xd8, 0xab, 0x00, 0x8c, 0xbc, 0xd3, 0x0a, 0xf7, 0xe4, 0x58, 0x05, 0xb8, 0xb3, 0x45, 0x06,
                    0xd0, 0x2c, 0x1e, 0x8f, 0xca, 0x3f, 0x0f, 0x02, 0xc1, 0xaf, 0xbd, 0x03, 0x01, 0x13, 0x8a, 0x6b,
                    0x3a, 0x91, 0x11, 0x41, 0x4f, 0x67, 0xdc, 0xea, 0x97, 0xf2, 0xcf, 0xce, 0xf0, 0xb4, 0xe6, 0x73,
                    0x96, 0xac, 0x74, 0x22, 0xe7, 0xad, 0x35, 0x85, 0xe2, 0xf9, 0x37, 0xe8, 0x1c, 0x75, 0xdf, 0x6e,
                    0x47, 0xf1, 0x1a, 0x71, 0x1d, 0x29, 0xc5, 0x89, 0x6f, 0xb7, 0x62, 0x0e, 0xaa, 0x18, 0xbe, 0x1b,
                    0xfc, 0x56, 0x3e, 0x4b, 0xc6, 0xd2, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0x78, 0xcd, 0x5a, 0xf4,
                    0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f,
                    0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef,
                    0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61,
                    0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d
            };


        }



    }
}
