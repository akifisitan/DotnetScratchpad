using System.Net.Http;
using System.Text;
using System.Windows;

namespace Netman;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var content = new StringContent(JsonTextBox.Text, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(UrlTextBox.Text, content);

            await Task.Delay(3000);
            var result = await response.Content.ReadAsStringAsync();
            ResponseTextBox.Text = result;
        }
        catch (Exception ex)
        {
            ResponseTextBox.Text = $"Error: {ex.Message}";
        }
    }
}
