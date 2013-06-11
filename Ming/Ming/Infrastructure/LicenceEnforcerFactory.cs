using Ming.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ming.Infrastructure
{
    internal static class LicenceEnforcerFactory
    {
        private static ILicenceEnforcer _instance;
        private static object _sync = new Object();

        public static ILicenceEnforcer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new LicenceEnforcer();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
