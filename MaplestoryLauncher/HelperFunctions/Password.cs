using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace MaplestoryLauncher
{
    public static class Password
    {
        static string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\MaplestoryLauncher\\UserState.dat";

        public static void Save(string password)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Create random entropy of 8 characters.
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                string entropy = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                Properties.Settings.Default.entropy = entropy;
                Properties.Settings.Default.Save();

                // Building ciphertext by 3DES.
                byte[] ciphertext = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                writer.Write(ciphertext);
            }
        }

        public static string Load()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    Byte[] cipher = File.ReadAllBytes(filePath);
                    string entropy = Properties.Settings.Default.entropy;
                    byte[] plaintext = ProtectedData.Unprotect(cipher, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(plaintext);
                }
                catch
                {
                    
                }
            }
            return String.Empty;
        }

        public static void Delete()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            Properties.Settings.Default.entropy = "";
            Properties.Settings.Default.Save();
        }
    }
}