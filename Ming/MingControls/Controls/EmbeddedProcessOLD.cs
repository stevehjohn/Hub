using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MingControls.Controls
{
    public delegate void StdOutReceievedEventHandler(string data);

    internal class EmbeddedProcessOLD
    {
        #region API Declarations

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct StartupInfo
        {
            public uint cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct ProcessInfo
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 ProcessId;
            public Int32 ThreadId;
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine, 
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, 
            bool bInheritHandles,
            uint dwCreationFlags, 
            IntPtr lpEnvironment, 
            string lpCurrentDirectory,
            [In] ref StartupInfo lpStartupInfo,
            out ProcessInfo lpProcessInformation);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public unsafe byte* lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [DllImport("kernel32.dll")]
        static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe,
            ref SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [Flags]
        enum HANDLE_FLAGS
        {
            INHERIT = 1,
            PROTECT_FROM_CLOSE = 2
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask,
            HANDLE_FLAGS dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern unsafe bool ReadFile(IntPtr hFile, void* lpBuffer,
           uint nNumberOfBytesToRead, int* lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer,
           uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten,
           IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);
        
        #endregion

        public event StdOutReceievedEventHandler StdOutDataReceived;

        private ProcessInfo _processInfo;

        IntPtr _hReadStdOut;
        IntPtr _hWriteStdIn;

        BackgroundWorker _stdOutReader;

        public void StartProcess(string pathToExeAndArgs)
        {
            CreateProcess(pathToExeAndArgs);

            _stdOutReader = new BackgroundWorker();
            _stdOutReader.DoWork += stdOutReaderDoWork;
            _stdOutReader.WorkerSupportsCancellation = true;
            _stdOutReader.RunWorkerAsync();
        }

        public void EndProcess()
        {
            _stdOutReader.CancelAsync();
            TerminateProcess(_processInfo.hProcess, 0);
        }

        public void SendToStdIn(string command)
        {
            var buf  = Encoding.UTF8.GetBytes(command);
            uint written;
            // TODO: If written < command.Length, write the rest
            var ok = WriteFile(_hWriteStdIn, buf, (uint)buf.Length, out written, IntPtr.Zero);
        }

        private void stdOutReaderDoWork(object sender, DoWorkEventArgs e)
        {
            var success = true;
            var buf = new byte[1024];
            int read;
            while (!_stdOutReader.CancellationPending && success)
            {
                read = 0;
                unsafe
                {
                    fixed (byte* bufPtr = buf)
                    {
                        success = ReadFile(
                            _hReadStdOut,
                            bufPtr,
                            1024,
                            &read,
                            IntPtr.Zero);
                    }
                }

                if (success)
                {
                    if (read < 1)
                    {
                        break;
                    }
                    var str = Encoding.UTF8.GetString(buf, 0, (int)read);
                    if (StdOutDataReceived != null)
                    {
                        StdOutDataReceived(str);
                    }
                }
            }
        }

        private void CreateProcess(string pathToExeAndArgs)
        {
            _processInfo = new ProcessInfo();

            bool ok = false;

            SECURITY_ATTRIBUTES sattr = new SECURITY_ATTRIBUTES();
            sattr.bInheritHandle = 1;
            unsafe
            {
                sattr.lpSecurityDescriptor = null;
            }
            sattr.nLength = Marshal.SizeOf(sattr);

            /*
            IntPtr hWrite;
            ok = CreatePipe(out _hReadStdOut, out hWrite, ref sattr, 0);
            ok = SetHandleInformation(_hReadStdOut, HANDLE_FLAGS.INHERIT, 0);
            IntPtr hRead;
            ok = CreatePipe(out hRead, out _hWriteStdIn, ref sattr, 0);
            ok = SetHandleInformation(_hWriteStdIn, HANDLE_FLAGS.INHERIT, 0);
             */
 
            var startInfo = new StartupInfo
            {
                dwFlags = 0x0001, // | 0x0100,
                wShowWindow = 0
                /*
                hStdOutput = hWrite,
                hStdError = hWrite,
                hStdInput = hRead
                 */
            };

            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
            tSec.nLength = Marshal.SizeOf(tSec);

            unsafe
            {
                ok = CreateProcess(
                    null,
                    pathToExeAndArgs,
                    ref pSec,
                    ref tSec,
                    true,
                    0,
                    IntPtr.Zero,
                    null,
                    ref startInfo,
                    out _processInfo);
            }
        }
    }
}
