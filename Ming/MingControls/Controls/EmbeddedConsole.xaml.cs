using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MingControls.Controls
{
    public partial class EmbeddedConsole : UserControl
    {
        private string _pathToExe;

        private FlowDocument _text;

        public EmbeddedConsole()
        {
            InitializeComponent();
            _text = ConsoleText;
        }

        private EmbeddedProcess _process;

        public void StartConsole(string pathToExe, string args)
        {
            _pathToExe = pathToExe;

            _process = new EmbeddedProcess();
            //_process.StdOutDataReceived += ProcessStdOutDataReceived;
            //_process.StartProcess("c:\\windows\\system32\\ping.exe localhost");
            _process.StartProcess(string.Format("{0} {1}", pathToExe, args));
            //_process.SendToStdIn("use CrashTest\r\n");
        }

        private void ProcessStdOutDataReceived(string data)
        {
            Write(data);
        }

        public void EndProcess()
        {
            _process.EndProcess();
        }

        private void Write(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            _text.Dispatcher.Invoke(new Action(() =>
                {
                    var para = new Paragraph(new Run(text));
                    para.Margin = new System.Windows.Thickness(0);
                    _text.Blocks.Add(para);
                }));
        }
    }
}
