using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Scratchpad.Lib;

public class WindowsClipboard
{
    private const uint CF_UNICODE_TEXT = 13;

    public bool IsSupported { get; } = CheckClipboardIsAvailable();

    private static bool CheckClipboardIsAvailable()
    {
        // Attempt to open the clipboard
        if (OpenClipboard(nint.Zero))
        {
            // Clipboard is available
            // Close the clipboard after use
            CloseClipboard();

            return true;
        }
        // Clipboard is not available
        return false;
    }

    public string GetClipboardData()
    {
        try
        {
            if (!OpenClipboard(nint.Zero))
            {
                return string.Empty;
            }

            nint handle = GetClipboardData(CF_UNICODE_TEXT);

            if (handle == nint.Zero)
            {
                return string.Empty;
            }

            nint pointer = nint.Zero;

            try
            {
                pointer = GlobalLock(handle);

                if (pointer == nint.Zero)
                {
                    return string.Empty;
                }

                int size = GlobalSize(handle);
                var buff = new byte[size];

                Marshal.Copy(pointer, buff, 0, size);

                return Encoding.Unicode.GetString(buff).TrimEnd('\0');
            }
            finally
            {
                if (pointer != nint.Zero)
                {
                    GlobalUnlock(handle);
                }
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    public void SetClipboardDataImpl(string text)
    {
        OpenClipboard();

        EmptyClipboard();
        nint hGlobal = default;

        try
        {
            int bytes = (text.Length + 1) * 2;
            hGlobal = Marshal.AllocHGlobal(bytes);

            if (hGlobal == default)
            {
                ThrowWin32();
            }

            nint target = GlobalLock(hGlobal);

            if (target == default)
            {
                ThrowWin32();
            }

            try
            {
                Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
            }
            finally
            {
                GlobalUnlock(target);
            }

            if (SetClipboardData(CF_UNICODE_TEXT, hGlobal) == default)
            {
                ThrowWin32();
            }

            hGlobal = default;
        }
        finally
        {
            if (hGlobal != default)
            {
                Marshal.FreeHGlobal(hGlobal);
            }

            CloseClipboard();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GlobalLock(nint hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GlobalSize(nint handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(nint hMem);

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    private void OpenClipboard()
    {
        var num = 10;

        while (true)
        {
            if (OpenClipboard(default))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            Thread.Sleep(100);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(nint hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetClipboardData(uint uFormat, nint data);

    private void ThrowWin32()
    {
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }
}
