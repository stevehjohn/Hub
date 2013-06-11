using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace MingPluginInterfaces
{
    public class ConnectionInfo
    {
        private readonly string _name;
        private readonly string _serviceId;
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly SecureString _password;

        public ConnectionInfo(string name, string serviceId, string host, int port, string username, SecureString password)
        {
            _name = name;
            _serviceId = serviceId;
            _host = host;
            _port = port;
            _username = username;
            _password = password;
        }

        public ConnectionInfo(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString");

            try
            {
                var parts = connectionString.Split(';');
                var found = 0;
                foreach (var part in parts)
                {
                        var keyValue = part.Split('=');
                        switch (keyValue[0].ToLower())
                        {
                            case "name":
                                if (!string.IsNullOrWhiteSpace(keyValue[1]))
                                {
                                    _name = keyValue[1];
                                }
                                break;
                            case "service":
                                found |= 1;
                                _serviceId = keyValue[1];
                                break;
                            case "host":
                                found |= 2;
                                _host = keyValue[1];
                                break;
                            case "port":
                                found |= 4;
                                _port = int.Parse(keyValue[1]);
                                break;
                            case "username":
                                if (!string.IsNullOrWhiteSpace(keyValue[1]))
                                {
                                    _username = keyValue[1];
                                }
                                break;
                            case "password":
                                if (!string.IsNullOrWhiteSpace(keyValue[1]))
                                {
                                    _password = Security.DecryptString(keyValue[1]);
                                }
                                break;
                        }
                }
                if ((found & 7) != 7)
                    throw new ArgumentException("Component not present parsing connection string", "connectionString");
            }
            catch
            {
                throw new ArgumentException("Error parsing connection string", "connectionString");
            }
        }

        public string Name { get { return _name; } }
        public string ServiceId { get { return _serviceId; } }
        public string Host { get { return _host; } }
        public int Port { get { return _port; } }
        public string Username { get { return _username; } }
        public SecureString Password { get { return _password; } }

        public override string ToString()
        {
            return string.Format("name={0};service={1};host={2};port={3};username={4};password={5}", _name, _serviceId, _host, _port, _username, 
                Security.EncryptString(_password));
        }
    }
}
