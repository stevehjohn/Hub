using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.Extensions
{
    public static class StringExtensions
    {
        public static int NthIndexOf(this string str, char value, int count)
        {
            int occurrence = 0;
            int pos = 0;
            foreach (char c in str)
            {
                if (c == value)
                {
                    occurrence++;
                }
                if (occurrence == count)
                {
                    return pos;
                }
                pos++;
            }
            
            return -1;
        }
    }
}
