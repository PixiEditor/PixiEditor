using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers.Extensions
{
    public static class EnumHelpers
    {
        public static IEnumerable<T> GetFlags<T>(this T e)
               where T : Enum
        {
            return Enum.GetValues(e.GetType()).Cast<T>().Where(x => e.HasFlag(x));
        }
    }
}