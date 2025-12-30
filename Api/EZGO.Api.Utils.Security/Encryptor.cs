using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

//TODO probably move to utils?
namespace EZGO.Api.Utils.Security
{
    /// <summary>
    /// See: https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
    /// See: https://docs.djangoproject.com/en/3.0/topics/auth/passwords/
    /// </summary>
    public class Encryptor
    {

        /// <summary>
        /// PBKDF2_Sha256_GetBytes; Django based password generation.
        /// Don't use the SHA1 here as in the documentation of Django describe this, but use the SHA256 here.
        /// I could't find the exact reason why Django uses this, but probably configured somewhere outside the code.
        /// </summary>
        /// <param name="dklen">Derivation Key should have a fixed length of 256 / 8 (32 bytes).</param>
        /// <param name="password">Plain text password to hash.</param>
        /// <param name="salt">3th section of the Django password string.</param>
        /// <param name="iterationCount">Based on current EZ-GO Django configuration 36000.</param>
        /// <returns>Byte array containing the hashed password.</returns>
        public static byte[] PBKDF2_Sha256_GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            using var hmac = new HMACSHA256(password);
            int hashLength = hmac.HashSize / 8;
            if ((hmac.HashSize & 7) != 0)
            {
                hashLength++;
            }

            int keyLength = dklen / hashLength;

            if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
            {
                throw new ArgumentOutOfRangeException("dklen");
            }

            if (dklen % hashLength != 0)
            {
                keyLength++;
            }

            byte[] subkey = new byte[salt.Length + 4];
            Buffer.BlockCopy(salt, 0, subkey, 0, salt.Length);
            using var ms = new System.IO.MemoryStream();

            for (int i = 0; i < keyLength; i++)
            {
                subkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                subkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                subkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                subkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);

                byte[] u = hmac.ComputeHash(subkey);
                Array.Clear(subkey, salt.Length, 4);
                byte[] f = u;

                for (int j = 1; j < iterationCount; j++)
                {
                    u = hmac.ComputeHash(u);
                    for (int k = 0; k < f.Length; k++)
                    {
                        f[k] ^= u[k];
                    }
                }

                ms.Write(f, 0, f.Length);
                Array.Clear(u, 0, u.Length);
                Array.Clear(f, 0, f.Length);
            }

            byte[] dk = new byte[dklen];
            ms.Position = 0;
            ms.Read(dk, 0, dklen);
            ms.Position = 0;

            for (long i = 0; i < ms.Length; i++)
            {
                ms.WriteByte(0);
            }

            Array.Clear(subkey, 0, subkey.Length);

            return dk;
        }

    }
}
