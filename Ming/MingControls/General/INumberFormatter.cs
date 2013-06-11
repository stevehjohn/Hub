using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingControls.General
{
    public interface INumberFormatter<T>
    {
        string Format(T value);
    }
}
