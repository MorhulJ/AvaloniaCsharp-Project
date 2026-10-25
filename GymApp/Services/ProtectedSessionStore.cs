using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace GymApp.Services;

internal static class ProtectedSessionStore
{
    private const uint UiForbidden = 0x1;

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int Size;
        public IntPtr Data;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptProtectData(ref DataBlob input, string? description,
        IntPtr entropy, IntPtr reserved, IntPtr prompt, uint flags, out DataBlob output);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description,
        IntPtr entropy, IntPtr reserved, IntPtr prompt, uint flags, out DataBlob output);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr memory);

    private static string SessionPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GymApp", "session.dat");

    public static bool Exists => File.Exists(SessionPath);

    internal sealed record StoredSession(string RefreshToken, string Email);

    public static void Save(string refreshToken, string email)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Protected sign-in is available on Windows only.");

        var clearText = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new StoredSession(refreshToken, email)));
        var input = ToBlob(clearText);
        try
        {
            if (!CryptProtectData(ref input, "GymApp session", IntPtr.Zero, IntPtr.Zero,
                    IntPtr.Zero, UiForbidden, out var output))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                var protectedBytes = new byte[output.Size];
                Marshal.Copy(output.Data, protectedBytes, 0, output.Size);
                Directory.CreateDirectory(Path.GetDirectoryName(SessionPath)!);
                File.WriteAllBytes(SessionPath, protectedBytes);
            }
            finally { LocalFree(output.Data); }
        }
        finally { Marshal.FreeHGlobal(input.Data); }
    }

    public static StoredSession? Load()
    {
        if (!File.Exists(SessionPath)) return null;
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Protected sign-in is available on Windows only.");

        var encrypted = File.ReadAllBytes(SessionPath);
        var input = ToBlob(encrypted);
        try
        {
            if (!CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                    IntPtr.Zero, UiForbidden, out var output))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                var clearText = new byte[output.Size];
                Marshal.Copy(output.Data, clearText, 0, output.Size);
                return JsonSerializer.Deserialize<StoredSession>(Encoding.UTF8.GetString(clearText));
            }
            finally { LocalFree(output.Data); }
        }
        finally { Marshal.FreeHGlobal(input.Data); }
    }

    public static void Delete()
    {
        if (File.Exists(SessionPath)) File.Delete(SessionPath);
    }

    private static DataBlob ToBlob(byte[] bytes)
    {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        return new DataBlob { Size = bytes.Length, Data = pointer };
    }
}
