using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MapleStoryLauncher
{
    internal class AccountManager
    {
        public class Settings
        {
            public bool rememberPassword = false;
            public bool autoLogin = false;
            public bool autoSelect = false;
            public string autoSelectAccount = default;
            public bool autoLaunch = false;
        }

        [JsonObject(MemberSerialization.Fields)]
        public class Account
        {
            private readonly string username;
            private readonly string entropy;
            private byte[] password;
            private readonly Settings settings;

            private static string CreateEntropy()
            {
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new();
                StringBuilder entropy = new(8);
                for (int i = 1; i <= 8; ++i)
                    entropy.Append(chars[random.Next(chars.Length)]);
                return entropy.ToString();
            }

            public Account(string username)
            {
                this.username = username;
                entropy = CreateEntropy();
                password = default;
                settings = new Settings();
            }

            public string Username { get { return username; } }

            public string Password
            {
                get
                {
                    if (password == default)
                        return "";
                    byte[] plaintext = ProtectedData.Unprotect(password, Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(plaintext);
                }
                set
                {
                    if (value == "" || value == default)
                        password = default;
                    else
                    {
                        byte[] cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(entropy), DataProtectionScope.CurrentUser);
                        password = cipher;
                    }
                }
            }

            public Settings Settings { get { return settings; } }
        }

        private readonly string savePath;
        private List<Account> accounts;

        public AccountManager(string path)
        {
            savePath = path;

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!File.Exists(path))
                     File.Create(path).Close();
            }
            catch
            {
                MessageBox.Show("權限不足，無法存取使用者資料檔案。");
                Environment.Exit(0);
            }

            try { accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(path)); }
            catch
            {
                if (MessageBox.Show("使用者資料損毀，要清除並繼續嗎？", "",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    Environment.Exit(0);
                else
                    accounts = null;
            }

            if(accounts == null)
                accounts = new List<Account>();
        }

        public List<string> GetListOfUsernames()
        {
            List<string> list = new(
                from acc in accounts
                select acc.Username
            );
            return list;
        }

        public bool Contains(string username)
        {
            foreach (Account account in accounts)
                if (account.Username == username)
                    return true;
            return false;
        }

        public void AddAccount(string username)
        {
            Account account = new(username);
            accounts.Add(account);
        }

        public void RemoveAccount(string username)
        {
            accounts.Remove(accounts.Find(account => account.Username == username));
        }

        public Account GetAccount(string username)
        {
            foreach (Account account in accounts)
                if (account.Username == username)
                    return account;
            return null;
        }

        public bool SaveToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(accounts);
                File.WriteAllText(savePath, json);
            }
            catch
            {
                MessageBox.Show("無法存取使用者資料檔案，請確認其沒有被其他程式鎖定。\n" +
                                "除非下次儲存成功，否則部分資料可能遺失！");
                return false;
            }
            return true;
        }
    }
}
