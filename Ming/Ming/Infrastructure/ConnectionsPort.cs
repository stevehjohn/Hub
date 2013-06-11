using System.IO;
using System.Linq;
using ServiceStack.Text;
using Microsoft.Win32;
using MingPluginInterfaces;

namespace Ming.Infrastructure
{
    internal class ConnectionsPort
    {
        public void ExportConnections()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = Properties.Resources.Export_DefaultFileName;
            dlg.DefaultExt = Properties.Resources.Export_DefaultExtension;
            dlg.Filter = Properties.Resources.Export_Filter;
            dlg.Title = Properties.Resources.Export_SaveTitle;

            if (dlg.ShowDialog() == true)
            {
                var cnns = SettingsManager.Instance.Connections.Select(
                    cnn => new JsonConnectionInfo() 
                    { 
                        Name = cnn.Name,
                        Host = cnn.Host, 
                        Port = cnn.Port, 
                        ServiceId = cnn.ServiceId,
                        Username = cnn.Username,
                        Password = Security.SecureStringToString(cnn.Password)
                    }).ToArray();

                var success = true;
                try
                {
                    var writer = new StreamWriter(dlg.FileName);
                    writer.Write(cnns.ToJson());
                    writer.Close();
                }
                catch 
                {
                    success = false;
                }
                if (success)
                {
                    MingApp.Instance.StatusMessage(Properties.Resources.Export_Success);
                }
                else
                {
                    MingApp.Instance.StatusMessage(Properties.Resources.Export_Error);
                }
            }
        }

        public void ImportConnections()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = Properties.Resources.Export_DefaultExtension;
            dlg.Filter = Properties.Resources.Export_Filter;
            dlg.Title = Properties.Resources.Import_OpenTitle;

            if (dlg.ShowDialog() == true)
            {
                var success = true;
                try
                {
                    var reader = new StreamReader(dlg.FileName);
                    string data = reader.ReadToEnd();
                    reader.Close();

                    var cnns = data.FromJson<JsonConnectionInfo[]>();

                    cnns.ToList().ForEach(
                        item =>  
                        {
                            try 
                            {
                                SettingsManager.Instance.AddConnection(new ConnectionInfo(item.Name, item.ServiceId, item.Host, item.Port, item.Username, Security.StringToSecureString(item.Password)));
                            } 
                            catch { }
                        });
                }
                catch
                {
                    success = false;
                }
                if (success)
                {
                    MingApp.Instance.StatusMessage(Properties.Resources.Import_Success);
                }
                else
                {
                    MingApp.Instance.StatusMessage(Properties.Resources.Import_Error);
                }
            }
        }
    }
}
