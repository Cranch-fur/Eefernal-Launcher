using System;
using System.Text;
using System.Runtime.InteropServices;






namespace EefernalLauncher
{
    public static class KERNEL32
    {
        public static class Struct
        {
            /*
                * https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfow
                * LayoutKind.Sequential - specifies that the fields of the managed structure should be laid out in memory in the same order they are declared, with no reordering.
                * CharSet.Unicode - indicates that string parameters should be marshaled as Unicode (UTF-16) when calling the unmanaged function.
            */
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STARTUPINFO
            {
                public uint cb;
                public string lpReserved;
                public string lpDesktop;
                public string lpTitle;
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


            /*
                * https://learn.microsoft.com/ru-ru/windows/win32/api/processthreadsapi/ns-processthreadsapi-process_information
                * LayoutKind.Sequential - specifies that the fields of the managed structure should be laid out in memory in the same order they are declared, with no reordering.
            */
            [StructLayout(LayoutKind.Sequential)]
            public struct PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public uint dwProcessId;
                public uint dwThreadId;
            }
        }






        /*
            * https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw
            * SetLastError - instructs the runtime to capture the Win32 error code (via GetLastError) immediately after the call, allowing to retrieve it in managed code with Marshal.GetLastWin32Error().
            * CharSet.Unicode - indicates that string parameters should be marshaled as Unicode (UTF-16) when calling the unmanaged function.
        */
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessW
        (
            string lpApplicationName,
            StringBuilder lpCommandLine, // StringBuilder for mutable buffer (CreateProcessW can modify the contents of this string).
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref Struct.STARTUPINFO lpStartupInfo,
            out Struct.PROCESS_INFORMATION lpProcessInformation
        );
    }
}
