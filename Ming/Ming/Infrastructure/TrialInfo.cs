using System;

namespace Ming.Infrastructure
{
    internal class TrialInfo
    {
        public Guid Guid { get; set; }

        public DateTime UtcDate { get; set; }

        public DateTime LocalDate { get; set; }

        public string Event { get; set; }
    }
}
