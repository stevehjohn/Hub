using Ming.Infrastructure.Interfaces;
using MingPluginInterfaces;
using ServiceStack.Text;
using System;
using System.Security.Cryptography;

namespace Ming.Infrastructure
{
    internal class LicenceEnforcer : ILicenceEnforcer
    {
        private RSAParameters _publicKey;

        private LicenceStatus _status = LicenceStatus.TrialExpired;

        private LicenceInfo _licenseInfo;

        public LicenceEnforcer()
        {
            _publicKey = new RSAParameters
            {
                Modulus = Convert.FromBase64String("4lQfTKBy16Wo/0XqbVKcC8qcFdnc+L4DcnMXY7a6AhdsaJ5GjdmS5MxNP1rCzIHENdJgYVPXZDXQ7OMlbO6WKw=="),
                Exponent = Convert.FromBase64String("AQAB")
            };

            DetermineLicenceStatus();
        }

        public LicenceStatus Status
        {
            get 
            {
                return _status;
            }
        }

        public int TrialDaysRemaining
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public bool IsValidModule(int moduleId)
        {
            throw new NotImplementedException();
        }

        private void DetermineLicenceStatus() 
        {
            LoadLiceseInfo();

            if (_licenseInfo == null)
            {
                if (SettingsManager.Instance.Connections.Count > 0) 
                {
                    _status = LicenceStatus.TrialExpired;
                    RecordTrialEvent("TAMPER");
                    return;
                }
                NewInstallation();
            }

            if (DateTime.UtcNow - _licenseInfo.InstallDate > new TimeSpan(14, 0, 0, 0))
            {
                _status = LicenceStatus.TrialExpired;

                if (!_licenseInfo.TrialExpired)
                {
                    _licenseInfo.TrialExpired = true;
                    SaveLicenceInfo();
                    RecordTrialEvent("EXPIRED");
                }

                return;
            }

            _status = LicenceStatus.TrialPeriod;
        }

        private void NewInstallation() 
        {
            _licenseInfo = new LicenceInfo();

            _licenseInfo.InstallDate = DateTime.UtcNow;
            _licenseInfo.TrialGuid = Guid.NewGuid();

            RecordTrialEvent("START");

            SaveLicenceInfo();
        }

        private void LoadLiceseInfo()
        {
            _licenseInfo = null;
            
            var strLicInfo = Properties.Settings.Default.LicenseInfo;

            if (!string.IsNullOrWhiteSpace(strLicInfo))
            {
                try
                {
                    _licenseInfo = Security.SecureStringToString(Security.DecryptString(strLicInfo)).FromJson<LicenceInfo>();
                } 
                catch 
                { 
                    _licenseInfo = null;
                }
            }
        }

        private void SaveLicenceInfo()
        {
            if (_licenseInfo == null) 
            { 
                return; 
            }

            Properties.Settings.Default.LicenseInfo = Security.EncryptString(Security.StringToSecureString(_licenseInfo.ToJson()));
            Properties.Settings.Default.Save();
        }

        private void RecordTrialEvent(string eventText)
        {
            var task = new CancelableTask(() => DoRecordTrialEvent(eventText), null);
            task.Execute();
        }

        private void DoRecordTrialEvent(string eventText)
        {
            try
            {
                LicensingCommunications.SendMessage<Acknowledgement, TrialInfo>("/api/firstexecution",
                    new TrialInfo
                    {
                        Guid = _licenseInfo.TrialGuid,
                        LocalDate = DateTime.Now,
                        UtcDate = DateTime.UtcNow,
                        Event = eventText
                    });
            }
            catch { }
        }

        public bool ValidateLicence(string emailAddress, string licence)
        {
            throw new NotImplementedException();
        }
    }
}
