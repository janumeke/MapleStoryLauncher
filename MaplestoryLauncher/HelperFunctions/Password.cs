using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Forms;

namespace MaplestoryLauncher
{
    public static class Password
    {
        public static void Save(string password)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(Application.UserAppDataPath + "\\MaplestoryLauncher\\UserState.dat", FileMode.Create)))
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
            string localAppDataPath = Environment.GetEnvironmentVariable("LocalAppData");
            if (File.Exists(localAppDataPath + "\\MaplestoryLauncher\\UserState.dat"))
            {
                try
                {
                    Byte[] cipher = File.ReadAllBytes(localAppDataPath + "\\MaplestoryLauncher\\UserState.dat");
                    string entropy = Properties.Settings.Default.entropy;
                    byte[] plaintext = ProtectedData.Unprotect(cipher, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(plaintext);
                }
                catch
                {
                    Delete();
                }
            }
            return String.Empty;
        }

        public static void Delete()
        {
            string localAppDataPath = Environment.GetEnvironmentVariable("LocalAppData");
            File.Delete(localAppDataPath + "\\MaplestoryLauncher\\UserState.dat");
            Properties.Settings.Default.entropy = "";
            Properties.Settings.Default.Save();
        }
    }
}