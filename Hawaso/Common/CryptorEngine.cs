using System;
using System.Security.Cryptography;
using System.Text;

namespace Hawaso.Common
{
    public class CryptorEngine
    {
        /// <summary>
        /// 단방향 암호화: 복호화 불가능 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            return SHA256Hash(MD5Hash(password));
        }

        /// <summary>
        /// MD5 암호화
        /// </summary>
        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Encoding.ASCII.GetString(result);
            }
        }

        /// <summary>
        /// SHA256 암호화
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SHA256Hash(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = 
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(
                    hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
