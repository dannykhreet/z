using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EZGO.Api.Utils.Security
{
    /// <summary>
    /// Authenticator; Authenticator is used a collection of methods for use with authenticating a used.
    /// More or less all functionality around a user name and password. Seeing we are using a django-based user portal some concessions are made to make sure we are compatible.
    /// Note password construction is based on : "{algorithm}${iterations}${salt}${hashedpassword}"
    /// </summary>
    public class Authenticator
    {
        public const string algorithm = "pbkdf2_sha256";
        public const int iterations = 36000;
        public const int saltlength = 12;

        /// <summary>
        /// GetSaltFromPassword; Get salt from hased password.
        /// </summary>
        /// <param name="hashedpassword">Password containing the salt.</param>
        /// <returns>Return salt item.</returns>
        public string GetSaltFromPassword(string hashedpassword)
        {
            string salt = string.Empty;
            if (!string.IsNullOrEmpty(hashedpassword))
            {
                var segments = hashedpassword.Split("$");
                if (segments.Length > 3)
                {
                    var possiblesalt = segments[2];
                    if (!string.IsNullOrEmpty(possiblesalt))
                    {
                        salt = possiblesalt;
                    }
                }
            }
            return salt;
        }

        /// <summary>
        /// GenerateNewSalt; generate a new SALT string.
        /// </summary>
        /// <returns>Salt string</returns>
        public string GenerateNewSalt()
        {
            var salt = new byte[12];
            var random = RandomNumberGenerator.Create();
            random.GetNonZeroBytes(salt);
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// GetIterationFromPassword; Get iteration structure from hashed password.
        /// </summary>
        /// <param name="hashedpassword">Password containing the salt.</param>
        /// <returns>iteration item.</returns>
        public int GetIterationFromPassword(string hashedpassword)
        {
            int iterations = Authenticator.iterations;
            if (!string.IsNullOrEmpty(hashedpassword))
            {
                var segments = hashedpassword.Split("$");
                if (segments.Length > 3)
                {
                    var possibleiteration = segments[1];
                    if (!string.IsNullOrEmpty(possibleiteration))
                    {
                        iterations = Convert.ToInt32(possibleiteration);
                    }
                }
            }
            return iterations;
        }

        /// <summary>
        /// GetString; Get string from byte.
        /// </summary>
        /// <param name="bytes">Bytes to get string from.</param>
        /// <returns>String based on the incoming byte.</returns>
        public string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// GetAlgoritmFromPassword; Get algorithm used from hashed password.
        /// </summary>
        /// <param name="hashedpassword">Password containing the salt.</param>
        /// <returns>Algorithm from password.</returns>
        public string GetAlgoritmFromPassword(string hashedpassword)
        {
            string algoritm = string.Empty;
            if (!string.IsNullOrEmpty(hashedpassword))
            {
                var segments = hashedpassword.Split("$");
                if (segments.Length > 3)
                {
                    var possiblealgoritm = segments[0];
                    if (!string.IsNullOrEmpty(possiblealgoritm))
                    {
                        algoritm = possiblealgoritm;
                    }
                }
            }
            return algoritm;
        }

        /// <summary>
        /// GenerateEncryptedPassword; Generate a crypted password as would be used for database storage based on a unencrypted password.
        /// </summary>
        /// <param name="unencryptedPassword">Password that needs to be encrypted.</param>
        /// <returns>An encrypted password.</returns>
        public string GenerateEncryptedPassword(string unencryptedPassword)
        {
            // Generate a SALT
            var newSalt = this.GenerateNewSalt();
            // Hash incoming password with current salt
            var newEncryptedPasswordByte = Encryptor.PBKDF2_Sha256_GetBytes(256 / 8, this.GetBytePassword(unencryptedPassword), this.GetBytePassword(newSalt), Authenticator.iterations);
            // create a full password including formatting
            var newGeneratedPassword = this.GeneratePasswordForStorage(newSalt, this.GetBase64PasswordHash(newEncryptedPasswordByte));

            return newGeneratedPassword;
        }

        /// <summary>
        /// GeneratePasswordForStorage; Get password for saving to database.
        /// </summary>
        /// <param name="salt">Salt string</param>
        /// <param name="hashedpassword">Hashed password string</param>
        /// <returns>A complete hashed password + algoritm + iteration settings + salt for database storage.</returns>
        public string GeneratePasswordForStorage(string salt, string hashedpassword)
        {
            return $"{algorithm}${iterations}${salt}${hashedpassword}";
        }

        /// <summary>
        /// GetStringSalt; Get string from byte.
        /// </summary>
        /// <param name="salt">Salt in bytes.</param>
        /// <returns>String (based on ASCII)</returns>
        public string GetStringSalt(byte[] salt)
        {
            //add validation
            return Encoding.ASCII.GetString(salt);
        }

        /// <summary>
        /// GetStringPassword; Get string from byte.
        /// </summary>
        /// <param name="password">Password in bytes</param>
        /// <returns>String (based on ASCII)</returns>
        public string GetStringPassword(byte[] password)
        {
            return Encoding.ASCII.GetString(password);
        }

        /// <summary>
        /// GetByteSalt; Get byte from string.
        /// </summary>
        /// <param name="salt">Salt in string</param>
        /// <returns>Byte</returns>
        public byte[] GetByteSalt(string salt)
        {
            //add validation
            return Encoding.ASCII.GetBytes(salt);
        }

        /// <summary>
        /// GetBytePassword; Get string from byte.
        /// </summary>
        /// <param name="password">Password in string</param>
        /// <returns>Byte</returns>
        public byte[] GetBytePassword(string password)
        {
            //add validation
            return Encoding.ASCII.GetBytes(password);
        }

        /// <summary>
        /// GetBase64PasswordHash; Get string from byte.
        /// </summary>
        /// <param name="hashedpassword">Hashed password in bytes</param>
        /// <returns>string in base64</returns>
        public string GetBase64PasswordHash(byte[] hashedpassword)
        {
            //add validation
            return Convert.ToBase64String(hashedpassword);
        }

    }
}
