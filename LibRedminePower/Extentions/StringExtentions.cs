using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class StringExtentions
    {
        /// <summary>
        /// 文字列を行ごとに分解した配列を返す。\r\n, \n, \r のいずれにも対応。
        /// </summary>
        public static string[] SplitLines(this string str)
        {
            return str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        /// <summary>
        /// 文字列を指定された行数まで短くして返す。オーバーする場合、末尾に「...」を追加する。
        /// </summary>
        public static string LimitRows(this string str, int rowMax, bool needsDeleteEmptyLine = false)
        {
            var lines = needsDeleteEmptyLine ?
                str.SplitLines().Where(l => !string.IsNullOrWhiteSpace(l)).ToList() :
                str.SplitLines().ToList();

            if (lines.Count() > rowMax)
            {
                lines = lines.Take(rowMax).ToList();
                lines.Add("...");
            }
            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// キャメルケース、パスカルケースをスネークケースに変換する。例）camelCase -> camel_case, PascalCase -> pascal_case
        /// </summary>
        public static string ToSnakeCase(this string str)
        {
            var regex = new System.Text.RegularExpressions.Regex("[a-z][A-Z]");
            return regex.Replace(str, s => $"{s.Groups[0].Value[0]}_{s.Groups[0].Value[1]}").ToLower();
        }

        private const string key = "MAKV2SPBNI99212";
        public static string Encrypt(this string clearText)
        {
            if (clearText == null) return null;

            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            try
            {
                return decrypt(cipherText);
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 複合化に失敗したら、もとの文字列を返す
        /// </summary>
        public static string DecryptOrDefault(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            try
            {
                return decrypt(cipherText);
            }
            catch (Exception)
            {
                return cipherText;
            }
        }

        private static string decrypt(string cipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (var encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
