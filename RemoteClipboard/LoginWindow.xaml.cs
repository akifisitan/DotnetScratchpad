using System.Windows;
using System.Windows.Controls;
using RemoteClipboard.Abstractions;
using RemoteClipboard.Model;

namespace RemoteClipboard;

public partial class LoginWindow : Window
{
    private readonly IAppService _appService;

    public LoginWindow()
    {
        InitializeComponent();

        _appService = DIContainer.GetRequiredService<IAppService>();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await HandleLogin();
    }

    private async Task HandleLogin()
    {
        try
        {
            ErrorMessage.Text = string.Empty;
            SetLoadingState(true);

            var userCredentials = new UserCredentials(
                UsernameTextBox.Text,
                PasswordBox.Password,
                ClipboardIdTextBox.Text
            );

            if (await _appService.Login(userCredentials))
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password";
                _appService.DeleteSavedCredentials();
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoginButton.IsEnabled = !isLoading;
        UsernameTextBox.IsEnabled = !isLoading;
        PasswordBox.IsEnabled = !isLoading;
        LoadingBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetLoadingState(true);
        var savedCredentials = await _appService.GetSavedCredentials();
        SetLoadingState(false);

        if (savedCredentials == null)
        {
            return;
        }

        UsernameTextBox.Text = savedCredentials.UserName;
        PasswordBox.Password = savedCredentials.Password;
        ClipboardIdTextBox.Text = savedCredentials.ClipboardId;

        await HandleLogin();
    }
}
