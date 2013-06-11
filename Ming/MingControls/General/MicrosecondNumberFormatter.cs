using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.General
{
    public class MicrosecondNumberFormatter : INumberFormatter<double>
    {
        public string Format(double value)
        {
            if (value >= 1000000)
            {
                return string.Format("{0:0.00} s", value / 1000000);
            }
            if (value >= 1000)
            {
                return string.Format("{0:0.00} ms", value / 1000);
            }
            return string.Format("{0} µs", value);
        }
    }
}
