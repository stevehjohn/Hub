using Ming.Forms;
using Ming.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ming.Controllers
{
    internal class LicenceEntryController
    {
        private LicenceEntry _form;
        private Window _owner;

        public LicenceEntryController(Window owner)
        {
            _form = new LicenceEntry();
            _owner = owner;
            _form.Owner = _owner;

            _form.ActivateOfflineButton.Click += ActivateOfflineButtonClick;
        }

        private void ActivateOfflineButtonClick(object sender, RoutedEventArgs e)
        {
            ValidateLicence(_form.EmailAddress.Text, _form.Licence.Text);
        }

        private void ValidateLicence(string emailAddress, string licence)
        {
            var le = LicenceEnforcerFactory.Instance;

            le.ValidateLicence(emailAddress, licence);
        }

        public void Show()
        {
            _form.ShowDialog();
        }
    }
}
