﻿using System;

namespace Ming.Infrastructure
{
    public class Licence
    {
        public string LicenseeEmailAddress { get; set; }

        public Guid LicenceId { get; set; }

        public int NumberOfLicences { get; set; }

        public string[] ValidModules { get; set; }
    }
}
