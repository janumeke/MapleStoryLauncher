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
        private class Account
        {
            public string username { get; set; }
            public string entropy { get; set; }
            public byte[] password { get; set; }
            public Settings settings { get; set; }
        }

        private string savePath;
        private List<Account> accounts;

        public AccountManager(string savePath)
        {
            this.savePath = savePath;
            if (!CheckSaveFile(savePath))
                File.Create(savePath);
            accounts = JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(savePath));
            if(accounts == null)
                accounts = new List<Account>();
        }

        public List<string> GetListOfUsernames()
        {
            List<string> list = new(
                from acc in accounts
                select acc.username
            );
            return list;
        }

        public bool Contains(string username)
        {
            foreach (Account account in accounts)
                if (account.username == username)
                    return true;
            return false;
        }

        public void AddAccount(string username)
        {
            Account account = new()
            {
                username = username,
                entropy = CreateEntropy(),
                password = default,
                settings = new()
                {
                    rememberPwd = false,
                    autoLogin = false,
                    autoSelect = false,
                    autoLaunch = false,
                }
            };
            accounts.Add(account);
        }

        public void RemoveAccount(string username)
        {
            accounts.Remove(accounts.Find(account => account.username == username));
        }

        public string GetPassword(string username)
        {
            Account account = accounts.Find((account) => account.username == username);
            if (account.password == default)
                return "";
            byte[] plaintext = ProtectedData.Unprotect(account.password, Encoding.UTF8.GetBytes(account.entropy), DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintext);
        }

        public void SavePassword(string username, string password)
        {
            Account account = accounts.Find((account) => account.username == username);
            if (password == "")
                account.password = default;
            else
            {
                byte[] cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(account.entropy), DataProtectionScope.CurrentUser);
                account.password = cipher;
            }
        }

        public class Settings
        {
            public bool rememberPwd { get; set; }
            public bool autoLogin { get; set; }
            public bool autoSelect { get; set; }
            public string autoSelectAccount { get; set; }
            public bool autoLaunch { get; set; }
        }

        public Settings GetSettings(string username)
        {
            Account account = accounts.Find((account) => account.username == username);
            return account != default ? account.settings : default;
        }

        public bool SaveSettings(string username, Settings settings)
        {
            Account account = accounts.Find((account) => account.username == username);
            if(account == default)
                return false;
            account.settings = settings;
            return true;
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
                return false;
            }
            return true;
        }

        public static bool CheckSaveFile(string path)
        {
            if(!File.Exists(path))
                return false;

            try
            {
                JsonConvert.DeserializeObject<List<Account>>(File.ReadAllText(path));
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static string CreateEntropy()
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new();
            StringBuilder entropy = new(8);
            for (int i = 1; i <= 8; ++i)
                entropy.Append(chars[random.Next(chars.Length)]);
            return entropy.ToString();
        }
    }
}
