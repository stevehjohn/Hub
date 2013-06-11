using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToIntOr0IfNaN(this double value)
        {
            if (value == double.NaN)
            {
                return 0;
            }
            return (int)value;
        }
    }
}
