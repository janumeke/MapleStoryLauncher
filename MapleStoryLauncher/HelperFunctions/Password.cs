using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace MapleStoryLauncher
{
    public static class Password
    {
        static readonly string filePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{typeof(MainWindow).Namespace}\\UserState.dat";

        public static bool Save(string password)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Create entropy of 8 random characters.
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new();
                StringBuilder entropy = new(8);
                for (int i = 1; i <= 8; ++i)
                {
                    entropy.Append(chars[random.Next(chars.Length)]);
                }
                Properties.Settings.Default.entropy = entropy.ToString();
                Properties.Settings.Default.Save();

                byte[] cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(entropy.ToString()), DataProtectionScope.CurrentUser);
                File.WriteAllBytes(filePath, cipher);
            }
            catch
            {
                Delete();
                return false;
            }
            return true;
        }

        public static string Load()
        {
            if (!File.Exists(filePath))
                return String.Empty;

            try
            {
                Byte[] cipher = File.ReadAllBytes(filePath);
                string entropy = Properties.Settings.Default.entropy;
                byte[] plaintext = ProtectedData.Unprotect(cipher, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plaintext);
            }
            catch
            {
                return String.Empty;
            }
        }

        public static bool Delete()
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                Properties.Settings.Default.entropy = "";
                Properties.Settings.Default.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}