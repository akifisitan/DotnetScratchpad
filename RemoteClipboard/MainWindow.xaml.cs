using System.Windows;

namespace RemoteClipboard;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ReadButton_Click(object sender, RoutedEventArgs e)
    {
        SetLoading(true);
        var result = await Functions.ReadFromRemoteClipboard();
        ReadTextBox.Text = result;
        //MessageBox.Show($"Clipboard data:\n{result}");
        SetLoading(false);
    }

    private async void WriteButton_Click(object sender, RoutedEventArgs e)
    {
        SetLoading(true);

        var text = !string.IsNullOrWhiteSpace(InputTextBox.Text) ? InputTextBox.Text : null;
        await Functions.WriteToRemoteClipboard(text);

        SetLoading(false);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Functions.SetClipboardText(ReadTextBox.Text);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        Functions.Logout();
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
