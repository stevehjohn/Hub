using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.General
{
    public class NumberFormatter : INumberFormatter<double>
    {
        public string Format(double value)
        {
            return value.ToString();
        }
    }
}
