using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EZGO.Api.Utils.Crypto
{
    /// <summary>
    /// Cryptography; Cryptography structure for encrypting and decrypting data.
    /// Normally we could use the Protection API within .net core 3.1 but that doesn't seem to function properly in our NAS and maybe ACCEPTANCE AND PRODUCTION environment.
    /// </summary>
    public class Cryptography : ICryptography
    {
        #region - private / constants -
        private readonly IConfigurationHelper _confighelper;//Configuration for getting the protection key that we use.
        private string cryptoKey { get; set; } //Key we are using (filled in contructor, based on configuration)

        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create(); //random number generator used for creating the vector that is used.

        private const int VECTOR_SIZE = 16;
        private const int VECTOR_LENGTH = 20;
        private const int KEY_SIZE = 32;
        #endregion

        #region - constructor(s) -
        public Cryptography(IConfigurationHelper configurationHelper)
        {
            _confighelper = configurationHelper;
            cryptoKey = _confighelper.GetValueAsString(AuthenticationSettings.PROTECTION_CONFIG_KEY);
        }
        #endregion

        #region - public methods -

        /// <summary>
        /// Encrypt; Encrypt a string value. Method will use AES encryption with a random vector and a key (located in config) for encrypting the data.
        /// A encrypted string can be decrypted with <see cref="Decrypt(string)">Decrypt</see> method.
        /// </summary>
        /// <param name="unprotectedValue">Unencrypted value that needs to be encrypted.</param>
        /// <returns>A encrypted text.</returns>
        public string Encrypt(string unprotectedValue)
        {
            if (string.IsNullOrEmpty(unprotectedValue))
            {
                throw new NullReferenceException("unprotected value is null.");
            };
            var encrypted = "";
            //Get byte array of string.
            byte[] unprotectedBytes = Encoding.Unicode.GetBytes(unprotectedValue);
            //Using Aes for encryption and decryption.
            using (Aes encryptor = Aes.Create())
            {
                //Create byte array (15) for use with vector.
                byte[] IV = new byte[VECTOR_SIZE - 1];
                //Create random vector by filling byte array with random bytes. (not using storage, adding vector to encrypted key).
                Random.GetBytes(IV);
                //Create derived bytes object based on vector and the internal crypto key.
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(this.cryptoKey, IV);
                //Create pseudo-random key. (randomness based on cryptokey and vector)
                encryptor.Key = pdb.GetBytes(KEY_SIZE);
                //Create pseudo-random vector. (randomness based on cryptokey and vector)
                encryptor.IV = pdb.GetBytes(VECTOR_SIZE);
                //Encrypt everything.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(unprotectedBytes, 0, unprotectedBytes.Length);
                        cs.Close();
                    }
                    //Build encrypted string, containing the vector and the encrypted text.
                    encrypted = Convert.ToBase64String(IV) + Convert.ToBase64String(ms.ToArray());
                }
            }
            return encrypted;
        }

        /// <summary>
        /// Decrypt; Decrypt a string value (encrypted). Method will use AES encryption with a random vector and a key (located in config) for decrypting the data.
        /// A decrypted string can be created with <see cref="Encrypt(string)">Encrypt</see> method.
        /// </summary>
        /// <param name="protectedValue">Encrypted value that needs to be decrypted.</param>
        /// <returns>A decrypted text.</returns>
        public string Decrypt(string protectedValue)
        {
            if(string.IsNullOrEmpty(protectedValue))
            {
                throw new NullReferenceException("protected value is null.");
            };
            var decrypted = "";
            //Get vector from encrypted string.
            byte[] IV = Convert.FromBase64String(protectedValue.Substring(0, VECTOR_LENGTH));
            //Get protected value from encrypted string, replace the spaces if any, with +.
            protectedValue = protectedValue.Substring(VECTOR_LENGTH).Replace(" ", "+");
            //Get cyptherBytes.
            byte[] protectedBytes = Convert.FromBase64String(protectedValue);
            //Using Aes for encryption and decryption.
            using (Aes encryptor = Aes.Create())
            {
                //Create derived bytes object based on vector and the internal crypto key.
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(this.cryptoKey, IV);
                //Create pseudo-random key. (randomness based on cryptokey and vector)
                encryptor.Key = pdb.GetBytes(KEY_SIZE);
                //Create pseudo-random vector. (randomness based on cryptokey and vector)
                encryptor.IV = pdb.GetBytes(VECTOR_SIZE);
                //Decrypt everything.
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(protectedBytes, 0, protectedBytes.Length);
                        cs.Close();
                    }
                    //Get decrypted string from memory stream.
                    decrypted = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return decrypted;
        }

        /// <summary>
        /// NOT YET IMPLEMENTED.
        /// </summary>
        /// <returns></returns>
        public  string GetRandomSalt()
        {
            throw new NotImplementedException("GetRandomSalt() is not yet implemented");
        }

        /// <summary>
        /// NOT YET IMPLEMENTED.
        /// </summary>
        /// <returns></returns>
        public string GetHash()
        {
            throw new NotImplementedException("GetHash() is not yet implemented");
        }
        #endregion
    }
}
