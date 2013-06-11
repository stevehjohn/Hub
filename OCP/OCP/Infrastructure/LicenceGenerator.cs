using OCP.ApiModel;
using ServiceStack.Text;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace OCP.Infrastructure
{
    internal class LicenceGenerator
    {
        private RSAParameters _keys;

        public LicenceGenerator()
        {
            _keys = new RSAParameters
            {
                Modulus = Convert.FromBase64String("4lQfTKBy16Wo/0XqbVKcC8qcFdnc+L4DcnMXY7a6AhdsaJ5GjdmS5MxNP1rCzIHENdJgYVPXZDXQ7OMlbO6WKw=="),
                Exponent = Convert.FromBase64String("AQAB"),
                P = Convert.FromBase64String("/XlLz1VvsjbeeE+df6hqpq1lxS/h4dpIsOZbfpQt778="),
                Q = Convert.FromBase64String("5JWRnrWsWlrMDiFrlyEtMkMIrYTkc+YEp4E1vAZQ9JU="),
                DP = Convert.FromBase64String("zfumWcUqUf845zh722P315+N1qLEw49qBygMLl8ovW0="),
                DQ = Convert.FromBase64String("L+t3Iql9X1fHjXLOJlmrKu1IpW/FoNJoyWDaDffZAt0="),
                InverseQ = Convert.FromBase64String("3QJxmm03dJijhJeJ6BTIvzEVppQ/gQRrw6DAw2oR6wI="),
                D = Convert.FromBase64String("HuJZzwwJ/9FUSVlSDw75ykYgnH65P5w2PjTkWTADG+uykGLXaKfSH5/5UAgnD9K5zyM+uJAqw6IiaBbwkzy4qQ==")
            };
        }

        public string NewLicence(string emailAddress, int quantity, string[] modules)
        {
            var licence = new Licence
            {
                LicenceId = Guid.NewGuid(),
                LicenseeEmailAddress = emailAddress,
                NumberOfLicences = quantity,
                ValidModules = modules
            };

            var licencePackage = new LicencePackage();

            var toSign = licence.ToJson().Select(c => (byte)c).ToArray();

            var crypto = new RSACryptoServiceProvider();
            crypto.ImportParameters(_keys);

            licencePackage.Signature = crypto.SignData(toSign, new SHA1CryptoServiceProvider());

            var licencePackageBytes = licencePackage.ToJson().Select(c => (byte)c).ToArray();

            return Convert.ToBase64String(licencePackageBytes);
        }
    }
}