using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ming.Infrastructure
{
    internal class LicenceInfo
    {
        public DateTime InstallDate { get; set; }

        public Guid TrialGuid { get; set; }

        public bool TrialExpired { get; set; }
    }
}
