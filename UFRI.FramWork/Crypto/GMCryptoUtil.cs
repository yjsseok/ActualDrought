/////////////////////////////////////////////////////////////////////////////////////
/// ◑ Solution 		: UFRI
/// ◑ Project			: UFRI.FrameWork
/// ◑ Class Name		: EncDecSupporter
/// ◑ Description		: 암/복호화 지원 클래스 
///
/// - 256 Bit 사용 권장
/// 
/// ◑ Revision History
/////////////////////////////////////////////////////////////////////////////////////
/// Date			Author		    Description
/////////////////////////////////////////////////////////////////////////////////////
/// 2017/12/27      GiMoon     First Draft
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public class GMCryptoUtil
    {
        /// <summary>
        /// AES_256 암호화
        /// </summary>
        /// <param name="Input">입력 스트링</param>
        /// <param name="key">암호화 키</param>
        /// <returns></returns>
        public static String AESEncrypt256(String Input, String key)
        {
            string sEmptyString = "";

            if (key.Length < 32)
            {
                for (int i = 0; i < 32 - key.Length; i++)
                {
                    sEmptyString += " ";
                }

                key = key + sEmptyString;
            }

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(Input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        /// <summary>
        /// AES_256 복호화
        /// </summary>
        /// <param name="Input">복호화 스트링</param>
        /// <param name="key">복호화 키</param>
        /// <returns></returns>
        public static String AESDecrypt256(String Input, String key)
        {
            string sEmptyString = "";

            if (key.Length < 32)
            {
                for (int i = 0; i < 32 - key.Length; i++)
                {
                    sEmptyString += " ";
                }

                key = key + sEmptyString;
            }

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(Input);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }

        /// <summary>
        /// AES_128 암호화
        /// </summary>
        /// <param name="Input">암호화 스트링</param>
        /// <param name="key">암호화 키</param>
        /// <returns></returns>
        public static String AESEncrypt128(String Input, String key)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(Input);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(PlainText, 0, PlainText.Length);
            cryptoStream.FlushFinalBlock();

            byte[] CipherBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);

            return EncryptedData;
        }

        /// <summary>
        /// AES_128 복호화
        /// </summary>
        /// <param name="Input">복호화 스트링</param>
        /// <param name="key">복호화 키</param>
        /// <returns></returns>
        public static String AESDecrypt128(String Input, String key)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(Input);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream(EncryptedData);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            byte[] PlainText = new byte[EncryptedData.Length];

            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

            return DecryptedData;
        }

        /// <summary>
        /// 복호화가 되지 않는 SHA512 로 enumeration만큼 반복하여 암호화 (결과는 Base64String)
        /// </summary>
        /// <param name="input">암호화 할 문자열</param>
        /// <param name="salt">암호화 문자열에 추가될 솔트</param>
        /// <param name="enumeration">반복 횟수 (반복 횟수가 달라지면 해쉬된 값도 달라지므로 주의)</param>
        /// <returns>암호화된 문자열 (Base64String)</returns>
        public static string SHA512Encrypt(string input, string salt, int enumeration = 1)
        {
            if (enumeration < 1)
                throw new Exception("암호화 횟수가 잘못 입력 되었습니다.");

            byte[] inputByte = Encoding.UTF8.GetBytes(input);
            byte[] saltByte = Encoding.UTF8.GetBytes(salt);

            byte[] encryptedByte = SHA512Encrypt(inputByte, saltByte, enumeration);
            string encryptedString = Convert.ToBase64String(encryptedByte);

            return encryptedString;
        }

        private static byte[] SHA512Encrypt(byte[] input, byte[] salt, int enumeration = 1)
        {
            if (enumeration < 1)
                throw new Exception("암호화 횟수가 잘못 입력 되었습니다.");

            byte[] merged = new byte[input.Length + salt.Length];
            input.CopyTo(merged, 0);
            salt.CopyTo(merged, input.Length);

            SHA512 sha512 = SHA512.Create();
            byte[] hashed = sha512.ComputeHash(merged);

            if (enumeration > 1)
                hashed = SHA512Encrypt(hashed, salt, enumeration - 1);

            return hashed;
        }
    }
}
