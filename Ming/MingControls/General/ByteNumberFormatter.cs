using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.General
{
    public class ByteNumberFormatter : INumberFormatter<double>
    {
        public string Format(double value)
        {
            if (value >= 1099511627776)
            {
                return string.Format("{0:0.00} TB", value / 1099511627776);
            }
            if (value >= 1073741824)
            {
                return string.Format("{0:0.00} GB", value / 1073741824);
            }
            if (value >= 1048576)
            {
                return string.Format("{0:0.00} MB", value / 1048576);
            }
            if (value >= 1024)
            {
                return string.Format("{0:0.00} kB", value / 1024);
            }
            return string.Format("{0} B", value);
        }
    }
}
