using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MaplestoryLauncher
{
    partial class BeanfunClient
    {
        public class Account
        {
            public string sacc;
            public string sotp;
            public string sname;
            public string screatetime;

            public Account()
            { this.sacc = null; this.sotp = null; this.sname = null; this.screatetime = null; }
            public Account(string sacc, string sotp, string sname, string screatetime = null)
            { this.sacc = sacc; this.sotp = sotp; this.sname = sname; this.screatetime = screatetime; }
        }

        public List<Account> accountList = new List<Account>();

        public void GetAccounts(bool fatal = true)
        {
            if (this.webtoken == null)
            { return; }

            Regex regex;
            string response = this.DownloadString("https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D" + service_code + "_" + service_region + "&web_token=" + webtoken, Encoding.UTF8);

            regex = new Regex("<div id=\"(\\w+)\" sn=\"(\\d+)\" name=\"([^\"]+)\"");
            this.accountList.Clear();
            foreach (Match match in regex.Matches(response))
            {
                if (match.Groups[1].Value == "" || match.Groups[2].Value == "" || match.Groups[3].Value == "")
                { continue; }
                accountList.Add(new Account(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
            }
            if (fatal && accountList.Count == 0)
            { this.errmsg = "LoginNoAccount"; return; }

            this.errmsg = null;
        }
    }
}
