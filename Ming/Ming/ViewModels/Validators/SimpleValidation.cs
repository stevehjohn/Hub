using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ming.ViewModels.Validation
{
    internal static class SimpleValidation
    {
        public static bool IsInt(string value)
        {
            int i;
            return int.TryParse(value, out i);
        }

        public static bool IsIntInRange(string value, int min, int max)
        {
            int i;
            if (int.TryParse(value, out i))
            {
                return i >= min && i <= max;
            }
            return false;
        }
    }
}
