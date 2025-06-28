using System.Diagnostics;

namespace Scratchpad.Lib;

public static class FileEnumerator
{
    public static void Demo(string[]? args = null)
    {
        var sw = Stopwatch.StartNew();

        var cts = new CancellationTokenSource(1000);

        static bool Hello(string path)
        {
            return path.Contains("venv")
                || path.Contains("node_modules")
                || path.Contains("bin")
                || path.Contains("obj");
        }

        static bool Hello2(string path)
        {
            string[] strings = ["venv", "node_modules", "bin", "obj"];

            return strings.Any(path.Contains);
        }

        var files = FileEnumerator.EnumerateFiles(
            @"C:\users\user\projects",
            includeFilePredicate: path => path.Contains("hello"),
            ignoreDirPredicate: Hello2,
            cancellationToken: cts.Token
        );

        foreach (var file in files)
        {
            Console.WriteLine(file);
        }

        //Console.WriteLine(files.Count());

        //Console.WriteLine(string.Join('\n', files));

        Console.WriteLine(sw.Elapsed.TotalMilliseconds);
    }

    public static IEnumerable<string> EnumerateFiles(
        string rootPath,
        Func<string, bool>? includeFilePredicate = null,
        Func<string, bool>? ignoreDirPredicate = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        includeFilePredicate ??= _ => true;
        ignoreDirPredicate ??= _ => false;

        var enumerationOptions = new EnumerationOptions { IgnoreInaccessible = true };

        var directoryStack = new Stack<string>();
        directoryStack.Push(rootPath);

        while (directoryStack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentPath = directoryStack.Pop();

            // Process files in current directory
            foreach (var filePath in Directory.EnumerateFiles(currentPath, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (includeFilePredicate(filePath))
                {
                    yield return filePath;
                }
            }

            // Add subdirectories to stack
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
