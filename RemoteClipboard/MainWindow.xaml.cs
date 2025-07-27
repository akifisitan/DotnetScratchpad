using System.Windows;
using RemoteClipboard.Abstractions;
using Scratchpad.Lib.Clipboard;

namespace RemoteClipboard;

public partial class MainWindow : Window
{
    private readonly IClipboardService _clipboardService;
    private readonly IAppService _appService;

    public MainWindow()
    {
        InitializeComponent();

        _clipboardService = DIContainer.GetRequiredService<IClipboardService>();
        _appService = DIContainer.GetRequiredService<IAppService>();
    }

    private async void ReadButton_Click(object sender, RoutedEventArgs e)
    {
        SetLoading(true);
        var result = await _clipboardService.ReadFromClipboard();
        ReadTextBox.Text = result;
        _appService.SetClipboardText(result);

        //MessageBox.Show($"Clipboard data:\n{result}");
        SetLoading(false);
    }

    private async void WriteButton_Click(object sender, RoutedEventArgs e)
    {
        SetLoading(true);

        var text = !string.IsNullOrWhiteSpace(InputTextBox.Text) ? InputTextBox.Text : null;

        text ??= _appService.GetClipboardText();

        await _clipboardService.WriteToClipboard(text);

        SetLoading(false);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        _appService.SetClipboardText(ReadTextBox.Text);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _appService.Logout();
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private void SetLoading(bool isLoading)
    {
        WriteButton.IsEnabled = !isLoading;
        ReadButton.IsEnabled = !isLoading;
        LogoutButton.IsEnabled = !isLoading;
    }
}
