using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ming.Infrastructure
{
    public class JsonConnectionInfo
    {
        public string Name { get; set; }
        public string ServiceId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
