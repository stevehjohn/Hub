using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin
{
    public delegate void MenuStateChangedEventHandler(string menuKey, bool enabled, int tabDocumentId);
}
