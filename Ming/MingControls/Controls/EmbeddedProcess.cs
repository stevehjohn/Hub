using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MingControls.Controls
{
    internal class EmbeddedProcess
    {
        #region p/Invoke Declarations

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
            public uint ProcessId;
            public Int32 ThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public unsafe byte* lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CHAR_INFO
        {
            [FieldOffset(0)]
            internal char UnicodeChar;
            [FieldOffset(0)]
            internal char AsciiChar;
            [FieldOffset(2)] 
            internal UInt16 Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SMALL_RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
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

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpReadRegion);
        
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateConsoleScreenBuffer(uint dwDesiredAccess,
            uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes, uint dwFlags,
            IntPtr lpScreenBufferData);
        
        #endregion

        private ProcessInfo _processInfo;

        private IntPtr _screenBuffer;

        public void StartProcess(string pathToExeAndArgs)
        {
            CreateProcess(pathToExeAndArgs);
            ReadConsoleBuffer();
        }

        public void EndProcess()
        {
            TerminateProcess(_processInfo.hProcess, 0);
        }

        private void ReadConsoleBuffer()
        {
            var buffer = Marshal.AllocHGlobal(25 * 80 * Marshal.SizeOf(typeof(CHAR_INFO)));

            var rect = new SMALL_RECT { Left = 0, Top = 0, Right = 79, Bottom = 24 };

            ReadConsoleOutput(_screenBuffer, buffer, new COORD { X = 80, Y = 25 }, new COORD { X = 0, Y = 0 }, ref rect);

            var sb = new StringBuilder(80 * 25);
            var ptr = buffer;
            for (int i = 0; i < 80 * 25; i++)
            {
                var ci = (CHAR_INFO)Marshal.PtrToStructure(ptr, typeof(CHAR_INFO));
                sb.Append(ci.UnicodeChar);
                ptr += Marshal.SizeOf(typeof(CHAR_INFO));
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

            _screenBuffer = CreateConsoleScreenBuffer(0x80000000 | 0x40000000, 0x01 | 0x02, ref sattr, 1, IntPtr.Zero);

            var startInfo = new StartupInfo
            {
                dwFlags = 0x0001,
                wShowWindow = 0,
                hStdOutput = _screenBuffer
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
