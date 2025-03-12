#region

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace Kharazmi.Helpers
{
    public static class HashHelper
    {
        /// <summary>
        /// Encrypt a string into a string using a password.
        /// Uses Encrypt(byte[], byte[], byte[]) 
        /// </summary>
        /// <param name="clearText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt(string clearText, string password = "eB%+?Cgkb9-v3BTpdhJD")
        {
            var clearBytes =
                Encoding.Unicode.GetBytes(clearText);

            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });

            // TODO Replace with PasswordDerivedBytes
            var encryptedData = Encrypt(clearBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));

            return Convert.ToBase64String(encryptedData);
        }

        private static byte[] Encrypt(byte[] clearData, byte[] key, byte[] iv)
        {
            if (key is null)
                key = new byte[]
                    {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24};
            if (iv is null)
                iv = new byte[] {65, 110, 68, 26, 69, 178, 200, 219};

            var ms = new MemoryStream();
            var alg = Rijndael.Create();

            alg.Key = key;
            alg.IV = iv;

            var cs = new CryptoStream(ms,
                alg.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(clearData, 0, clearData.Length);

            cs.Close();

            var encryptedData = ms.ToArray();

            return encryptedData;
        }


        /// <summary>
        /// Encrypt bytes into bytes using a password.Uses Encrypt(byte[], byte[], byte[]) 
        /// </summary>
        /// <param name="clearData"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] clearData, string password)
        {
            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });
            // TODO Replace with PasswordDerivedBytes
            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));
        }

        /// <summary>
        /// Encrypt a file into another file using a password 
        /// </summary>
        /// <param name="fileIn"></param>
        /// <param name="fileOut"></param>
        /// <param name="password"></param>
        public static void Encrypt(string fileIn, string fileOut, string password)
        {
            var fsIn = new FileStream(fileIn,
                FileMode.Open, FileAccess.Read);
            var fsOut = new FileStream(fileOut,
                FileMode.OpenOrCreate, FileAccess.Write);

            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });

            var alg = Rijndael.Create();
            // TODO Replace with PasswordDerivedBytes
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            var cs = new CryptoStream(fsOut,
                alg.CreateEncryptor(), CryptoStreamMode.Write);

            const int bufferLen = 4096;
            var buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            cs.Close();
            fsIn.Close();
        }


        /// <summary>
        ///  Decrypt a string into a string using a password. Uses Decrypt(byte[], byte[], byte[]) 
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string password = "eB%+?Cgkb9-v3BTpdhJD")
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
                    0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });

            // TODO Replace with PasswordDerivedBytes
            var decryptedData = Decrypt(cipherBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));

            return Encoding.Unicode.GetString(decryptedData);
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] key, byte[] iv)
        {
            if (key is null)
                key = new byte[]
                    {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24};
            if (iv is null)
                iv = new byte[] {65, 110, 68, 26, 69, 178, 200, 219};

            var ms = new MemoryStream();
            var alg = Rijndael.Create();

            alg.Key = key;
            alg.IV = iv;

            var cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(cipherData, 0, cipherData.Length);

            cs.Close();

            var decryptedData = ms.ToArray();

            return decryptedData;
        }


        /// <summary>
        /// Decrypt bytes into bytes using a password. Uses Decrypt(byte[], byte[], byte[]) 
        /// </summary>
        /// <param name="cipherData"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] cipherData, string password)
        {
            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });
            // TODO Replace with PasswordDerivedBytes
            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));
        }


        /// <summary>
        /// Decrypt a file into another file using a password 
        /// </summary>
        /// <param name="fileIn"></param>
        /// <param name="fileOut"></param>
        /// <param name="password"></param>
        public static void Decrypt(string fileIn, string fileOut, string password)
        {
            var fsIn = new FileStream(fileIn,
                FileMode.Open, FileAccess.Read);
            var fsOut = new FileStream(fileOut,
                FileMode.OpenOrCreate, FileAccess.Write);

            var pdb = new PasswordDeriveBytes(password,
                new byte[]
                {
                    0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
                    0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
                });
            var alg = Rijndael.Create();
            // TODO Replace with PasswordDerivedBytes
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            var cs = new CryptoStream(fsOut,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            const int bufferLen = 4096;
            var buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                bytesRead = fsIn.Read(buffer, 0, bufferLen);
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            cs.Close();
            fsIn.Close();
        }
    }
}