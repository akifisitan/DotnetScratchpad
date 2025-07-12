using System.Windows;
using System.Windows.Input;

namespace RemoteClipboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        await Task.Delay(1000);
        var clipboardText = Clipboard.GetText(TextDataFormat.UnicodeText);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var clipboardText = Clipboard.GetText(TextDataFormat.UnicodeText);
            MessageBox.Show($"Text: {clipboardText}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show("An error occurred while getting clipboard text.");
        }
    }
}
