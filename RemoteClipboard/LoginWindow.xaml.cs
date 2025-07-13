using System.Windows;
using System.Windows.Controls;

namespace RemoteClipboard;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
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

            var userCredentials = new UserCredentials(UsernameTextBox.Text, PasswordBox.Password);

            if (await Functions.Login(userCredentials))
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Close();
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password";
                SecureStorage.DeleteCredentials();
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
        var savedCredentials = await SecureStorage.GetCredentials();
        SetLoadingState(false);

        if (savedCredentials == null)
        {
            return;
        }

        UsernameTextBox.Text = savedCredentials.UserName;
        PasswordBox.Password = savedCredentials.Password;

        await HandleLogin();
    }
}
