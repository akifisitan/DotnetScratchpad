using System.ComponentModel.DataAnnotations;
using ConsoleAppFramework;

namespace LogGrep;

public sealed class AppCommands
{
    [Command("")]
    public async Task Hello([EmailAddress] [Argument] string emailAddress, [Path] string path)
    {
        await Task.Delay(100);
        Console.WriteLine(emailAddress);
    }
}

internal class MyFilter(ConsoleAppFilter next) : ConsoleAppFilter(next)
{
    public override Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class PathAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var path = value?.ToString();
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return Path.IsPathFullyQualified(path) && File.Exists(path);
    }

    //public override string FormatErrorMessage(string name)
    //{
    //    return $"Wowzer {name}";
    //}
}
