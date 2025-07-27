using GlobalHotKeys;

namespace RemoteClipboard.Model;

public static class DesktopContext
{
    public static UserCredentials? UserCredentials { get; set; }
    public static HotKeyManager HotKeyManager { get; } = new();
}
