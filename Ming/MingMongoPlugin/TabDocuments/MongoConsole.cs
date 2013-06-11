using MingMongoPlugin.TabDocuments.UserControls;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MingMongoPlugin.TabDocuments
{
    internal class MongoConsole : ITabDocument
    {
        private readonly EvalJSControl _control;
        private readonly Process _process;

        private int _instanceId;
        private StreamWriter _stdInput;

        private readonly ConnectionInfo _cnn;

        public MongoConsole(string fullPathToexe, ConnectionInfo cnnInfo, string database)
        {
            _control = new EvalJSControl();
            _control.EvaluateClicked += ControlEvaluateClicked;
            _cnn = cnnInfo;

            LoadSettings();

            _control.Output.Text = string.Format("{0}\n", Properties.Resources.Console_AltEnterTip);
            _control.Output.Text += string.Format("{0}\n\n", Properties.Resources.Console_ClearTip);

            try
            {
                var psi = new ProcessStartInfo(fullPathToexe);
                // TODO: Username/password
                psi.Arguments = string.Format("{1}:{2}", fullPathToexe, _cnn.Host, _cnn.Port);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                _process = new Process();
                _process.StartInfo = psi;
                _process.OutputDataReceived += ProcessOutputDataReceived;
                _process.ErrorDataReceived += ProcessOutputDataReceived;
                _process.Exited += ProcessExited;
                _control.PreviewKeyUp += ControlPreviewKeyUp;
                _process.Start();
                _stdInput = _process.StandardInput;
                _process.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                _control.Output.Text = Properties.Resources.Console_ErrorOpening;
                // TODO: This setting isn't this classes responsibility
                Properties.Settings.Default.MongoExeLocation = string.Empty;
                Properties.Settings.Default.Save();
                Utilities.LogException(ex);
            }

            if (!string.IsNullOrEmpty(database))
            {
                _stdInput.WriteLine(string.Format("use {0}", database));
            }
        }

        private void LoadSettings()
        {
            var col = FromString(Properties.Settings.Default.ConsoleForeground);
            if (col.HasValue)
            {
                _control.ForegroundColor  = col.Value;
            }
            col = FromString(Properties.Settings.Default.ConsoleBackground);
            if (col.HasValue)
            {
                _control.BackgroundColor = col.Value;
            }
        }

        private Color? FromString(string sCol)
        {
            if (string.IsNullOrEmpty(sCol))
            {
                return null;
            }

            try
            {
                var col = (Color)ColorConverter.ConvertFromString(sCol);
                return col;
            }
            catch 
            {
                return null;
            }
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.ConsoleForeground = _control.ForegroundColor.ToString();
            Properties.Settings.Default.ConsoleBackground = _control.BackgroundColor.ToString();
            Properties.Settings.Default.Save();
        }

        private void ControlPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.System)
            {
                if (e.SystemKey == System.Windows.Input.Key.Return)
                {
                    ControlEvaluateClicked();
                }
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ControlEvaluateClicked()
        {
            if (_control.JavaScript.Text.Trim().ToLower() == "clear")
            {
                _control.Output.Text = string.Empty;
            }
            else
            {
                _control.Output.Text += '\n';
                _stdInput.WriteLine(_control.JavaScript.Text.Replace("\r", "").Replace("\n", "") + "\r\n\r\n\r\n");
            }
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _control.Dispatcher.Invoke(new Action(() => 
                {
                    _control.Output.Text += e.Data + '\n';
                    _control.Output.ScrollToEnd();
                }));
        }

        public System.Windows.Controls.UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return string.Format(Properties.Resources.Console_Title, _cnn.Host, _cnn.Port); }
        }

        public string Description
        {
            get { return string.Format(Properties.Resources.Console_Description, _cnn.Host, _cnn.Port); }
        }

        public bool CloseRequested()
        {
            SaveSettings();
            try
            {
                _process.Kill();
            }
            catch { }
            return true;
        }

        public int InstanceId
        {
            set { _instanceId = value; }
        }

        public void MenuItemClicked(string menuKey)
        {
            throw new NotImplementedException();
        }
    }
}
