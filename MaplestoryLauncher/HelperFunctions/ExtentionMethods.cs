using System;

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
    }
}