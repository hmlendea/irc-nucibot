using System;
using System.Collections.Generic;
using System.Text;

namespace IRC_Client.Formatting
{
    public enum ColorCode
    {
        White = 0,
        Black = 1,
        Navy = 2,
        Green = 3,
        Red = 4,
        Brown = 5,
        Purple = 6,
        Orange = 7,
        Lime = 9,
        Cyan = 10,
        Aqua = 11,
        Blue = 12,
        Pink = 13,
        Grey = 14,
        Silver = 15
    }

    static class IrcFormatting
    {
        public static string Bold(string text)
        { return (char)2 + text + (char)15; }

        public static string Color(string text, ColorCode foreColor)
        { return (char)3 + ((int)foreColor).ToString() + text + (char)3; }

        public static string Color(string text, ColorCode foreColor, ColorCode backColor)
        { return (char)3 + ((int)backColor).ToString() + "," + ((int)foreColor).ToString() + text + (char)3; }

        public static string ClearFormatting(string text)
        {
            return text.Trim()
                .Replace(((char)1).ToString(), "")
                .Replace(((char)2).ToString(), "")
                .Replace(((char)3).ToString(), "")
                .Replace(((char)15).ToString(), "");
        }
    }
}
