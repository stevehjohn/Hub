﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingPluginInterfaces
{
    public interface IConnectionProvider
    {
        ConnectionInfo ConnectionInfo { get; }
    }
}
