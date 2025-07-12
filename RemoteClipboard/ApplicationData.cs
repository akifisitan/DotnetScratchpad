namespace RemoteClipboard;

public static class ApplicationData
{
    public static UserCredentials? UserCredentials { get; set; }
}

public record UserCredentials(string UserName, string Password);
