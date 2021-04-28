using System;
using System.Linq;

namespace PixiEditor.Helpers
{
    public static class StringExtensions
    {
        public static string Reverse(this string s)
        {
            return new string(s.Reverse<char>().ToArray());
        }
    }
}