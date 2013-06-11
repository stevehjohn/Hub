using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace MingPluginInterfaces
{
    public static class Security
    {
        private static byte[] entropy = Encoding.Unicode.GetBytes("d2ff205e-e47c-40c9-874f-86cc81ebc2cb");

        public static string EncryptString(SecureString data)
        {
            if (data == null || data.Length == 0)
                return null;

            var encrypted = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(SecureStringToString(data)),
                entropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static SecureString DecryptString(string base64data)
        {
            if (base64data == null || base64data.Length == 0)
                return null;

            var decrypted = ProtectedData.Unprotect(
                Convert.FromBase64String(base64data),
                entropy,
                DataProtectionScope.CurrentUser);
            return StringToSecureString(Encoding.Unicode.GetString(decrypted));
        }

        public static string SecureStringToString(SecureString data)
        {
            if (data == null)
                return null;

            IntPtr unmanaged = IntPtr.Zero;
            try
            {
                unmanaged = Marshal.SecureStringToGlobalAllocUnicode(data);
                return Marshal.PtrToStringUni(unmanaged);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanaged);
            }
        }

        public static SecureString StringToSecureString(string data)
        {
            if (data == null)
                return null;

            unsafe
            {
                fixed (char* chars = data)
                {
                    var secure = new SecureString(chars, data.Length);
                    secure.MakeReadOnly();
                    return secure;
                }
            }
        }
    }
}
