using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Canvasser.Extensions
{
    public static class StringExtensions
    {
        public static string ToSurnameCase(this string raw)
        {
            var builder = new StringBuilder();
            bool wantsACapital = true;
            foreach (var c in raw)
            {
                if (wantsACapital)
                    builder.Append(new String(new [] {c}).ToUpper());
                else
                    builder.Append(new String(new [] {c}).ToLower());
                wantsACapital = !Char.IsLetter(c);
            }
            return builder.ToString();
        }
    }
}
