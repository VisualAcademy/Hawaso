using System.Security.Cryptography;
using System.Text;
using Azunt.Models;

namespace Azunt.Utils
{
    public static class LogKey
    {
        public static string MakeKey(AppLog x)
        {
            var payload =
                $"{x.TimeStamp?.UtcDateTime.ToString("o")}|{x.Level}|{x.Message}|{x.MessageTemplate}|{x.Exception}|{x.Properties}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString(); // hex
        }
    }
}
