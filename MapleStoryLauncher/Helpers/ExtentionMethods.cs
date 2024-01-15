using System;
using System.Net.Mail;

namespace ExtentionMethods
{
    public static class Extentions
    {
        public static bool IsAllDigits(this string str)
        {
            foreach (char ch in str)
                if (!Char.IsDigit(ch))
                    return false;
            return true;
        }

        public static bool IsEmail(this string str)
        {
            try { new MailAddress(str); }
            catch { return false; }
            return true;
        }

        public static ListViewItem FindItemWithSubItemTextExact(this ListView listView, string text)
        {
            return (
                from ListViewItem itemE in listView.Items
                where itemE.SubItems[0].Text == text
                select itemE
            ).FirstOrDefault();
        }
    }
}