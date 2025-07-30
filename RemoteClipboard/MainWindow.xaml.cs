using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard;

public class ClipboardItem
{
    public string Text { get; set; }
    public string DisplayText { get; set; }

    public ClipboardItem(string text)
    {
        Text = text;
        DisplayText = text.Length > 100 ? text[..100] + "..." : text;
    }
}

public partial class MainWindow : Window
{
    private readonly IClipboardService _clipboardService;
    private readonly IAppService _appService;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ObservableCollection<ClipboardItem> _clipboardItems = [];

    IRegistration? hotkeyManagerCtrlShiftCRegistration;
    IRegistration? hotkeyManagerCtrlShiftDelRegistration;
    IDisposable? hotKeyPressedEventSubscription;

    public MainWindow()
    {
        InitializeComponent();

        _clipboardService = DIContainer.GetRequiredService<IClipboardService>();
        _appService = DIContainer.GetRequiredService<IAppService>();

        ClipboardListView.ItemsSource = _clipboardItems;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        hotkeyManagerCtrlShiftCRegistration = DesktopContext.HotKeyManager.Register(
            VirtualKeyCode.KEY_C,
            Modifiers.Control | Modifiers.Shift | Modifiers.NoRepeat
        );

        hotkeyManagerCtrlShiftDelRegistration = DesktopContext.HotKeyManager.Register(
            VirtualKeyCode.VK_BACK,
            Modifiers.Control | Modifiers.Shift | Modifiers.NoRepeat
        );

        hotKeyPressedEventSubscription = DesktopContext.HotKeyManager.HotKeyPressed.Subscribe(
            async hotKey =>
            {
                if (!await _semaphoreSlim.WaitAsync(0))
                {
                    return;
                }

                if (hotKey.Key == VirtualKeyCode.KEY_C)
                {
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        try
                        {
                            await HandleRead();
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    });
                }
                else if (hotKey.Key == VirtualKeyCode.VK_BACK)
                {
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        try
                        {
                            await Task.Delay(1000);
                            MessageBox.Show("HALLOOOO");
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    });
                }
            }
        );
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        hotkeyManagerCtrlShiftCRegistration?.Dispose();
        hotkeyManagerCtrlShiftDelRegistration?.Dispose();

        hotKeyPressedEventSubscription?.Dispose();
    }

    private async void ReadButton_Click(object sender, RoutedEventArgs e)
    {
        await HandleRead();
    }

    private async void WriteButton_Click(object sender, RoutedEventArgs e)
    {
        await HandleWrite();
    }

    private void CopyItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string text)
        {
            _appService.SetClipboardText(text);
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        HandleLogout();
    }

    private void HandleLogout()
    {
        _appService.Logout();
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private async Task HandleWrite()
    {
        SetLoading(true);

        var text = !string.IsNullOrWhiteSpace(InputTextBox.Text) ? InputTextBox.Text : null;

        text ??= _appService.GetClipboardText();

        await _clipboardService.WriteToClipboard(text);

        SetLoading(false);
    }

    private async Task HandleRead()
    {
        SetLoading(true);

        var result = await _clipboardService.ReadLastNEntriesFromClipboard(5);

        if (result.Count > 0)
        {
            _clipboardItems.Clear();

            foreach (var item in result)
            {
                _clipboardItems.Add(new ClipboardItem(item));
            }

            _appService.SetClipboardText(result[0]);
        }

        SetLoading(false);
    }

    private void SetLoading(bool isLoading)
    {
        WriteButton.IsEnabled = !isLoading;
        ReadButton.IsEnabled = !isLoading;
        LogoutButton.IsEnabled = !isLoading;
        ClipboardListView.IsEnabled = !isLoading;
    }
}
