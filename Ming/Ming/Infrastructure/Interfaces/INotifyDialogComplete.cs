using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ming.Infrastructure.Interfaces
{
    public delegate void DialogCompleteEventHandler(object sender);

    internal interface INotifyDialogComplete
    {
        event DialogCompleteEventHandler DialogComplete;
    }
}
