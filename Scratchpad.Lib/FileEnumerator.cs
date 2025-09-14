using System.Diagnostics;

namespace Scratchpad.Lib;

public static class FileEnumerator
{
    public static IEnumerable<string> EnumerateFiles(
        string rootPath,
        Func<string, bool>? includeFilePredicate = null,
        Func<string, bool>? ignoreDirPredicate = null,
        EnumerationOptions? enumerationOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        includeFilePredicate ??= _ => true;
        ignoreDirPredicate ??= _ => false;

        enumerationOptions ??= new EnumerationOptions { IgnoreInaccessible = true };

        var directoryStack = new Stack<string>();
        directoryStack.Push(rootPath);

        while (directoryStack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPath = directoryStack.Pop();

            foreach (var filePath in Directory.EnumerateFiles(currentPath, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (includeFilePredicate(filePath))
                {
                    yield return filePath;
                }
            }

            foreach (
                var dirPath in Directory.EnumerateDirectories(currentPath, "*", enumerationOptions)
            )
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!ignoreDirPredicate(dirPath))
                {
                    directoryStack.Push(dirPath);
                }
            }
        }
    }
}
