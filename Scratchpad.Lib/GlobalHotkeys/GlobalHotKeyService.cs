using GlobalHotKeys;
using GlobalHotKeys.Native.Types;

namespace Scratchpad.Lib.GlobalHotkeys;

internal class GlobalHotKeyService : IDisposable
{
    private readonly HotKeyManager hotKeyManager = new();
    private readonly List<IDisposable?> hotKeyRegistrations = [];
    private IDisposable? observableHandle;

    public GlobalHotKeyService()
    {
        InitializeHotKeyManager();
    }

    public void Dispose()
    {
        observableHandle?.Dispose();
        foreach (var registration in hotKeyRegistrations)
        {
            registration?.Dispose();
        }
        hotKeyManager.Dispose();
    }

    private void InitializeHotKeyManager()
    {
        hotKeyRegistrations.Add(
            hotKeyManager.Register(
                VirtualKeyCode.KEY_A,
                Modifiers.Control | Modifiers.Shift | Modifiers.NoRepeat
            )
        );

        observableHandle = hotKeyManager.HotKeyPressed.Subscribe(async hotKey =>
        {
            await Task.Delay(199);

            Console.WriteLine($"Hotkey pressed: {hotKey}");

            if (hotKey.Key == VirtualKeyCode.KEY_A)
            {
                Console.Beep(1000, 2000);
            }
        });
    }
}
