using System.Diagnostics;
using System.Text;

namespace Scratchpad.Lib;

public static class PipelineUtils
{
    public static Process StartProcessAndReadOutput(
        string filename,
        string arguments,
        string workingDirectory
    )
    {
        var process =
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                }
            ) ?? throw new Exception("An error occurred while starting process");

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    public static void CloneRepository(
        string repoUrl,
        string branchName,
        string workingDirectory,
        string? directoryName = null
    )
    {
        var sw = Stopwatch.StartNew();

        using var process = StartProcessAndReadOutput(
            "git",
            $"clone --single-branch --branch {branchName} --depth 1 --no-tags {repoUrl} {directoryName ?? string.Empty}",
            workingDirectory
        );

        process.WaitForExit();

        Console.WriteLine($"Time taken: {sw.Elapsed.TotalSeconds} seconds");
    }

    public static void NugetRestore(
        string solutionPath,
        string workingDirectory = "",
        string configFilePath = "",
        string packagesDirectoryPath = ""
    )
    {
        var sw = Stopwatch.StartNew();

        var commandBuilder = new StringBuilder(
            $"restore {solutionPath} -NoCache -Verbosity Detailed -NonInteractive"
        );

        if (!string.IsNullOrWhiteSpace(configFilePath))
        {
            commandBuilder.Append($" -ConfigFile {configFilePath}");
        }

        if (!string.IsNullOrWhiteSpace(packagesDirectoryPath))
        {
            commandBuilder.Append($" -PackagesDirectory {packagesDirectoryPath}");
        }

        using var process = StartProcessAndReadOutput(
            "nuget",
            commandBuilder.ToString(),
            workingDirectory
        );

        process.WaitForExit();

        Console.WriteLine($"Time taken: {sw.Elapsed.TotalSeconds} seconds");
    }

    public static void CleanSolution(
        string baseDirectory,
        bool deletePackages = false,
        bool printDeleted = false
    )
    {
        var sw = Stopwatch.StartNew();

        var objBinDirectoryList = Directory.EnumerateDirectories(
            baseDirectory,
            "*",
            SearchOption.AllDirectories
        );

        if (deletePackages)
        {
            objBinDirectoryList = objBinDirectoryList.Where(x =>
                Path.GetFileName(x).Equals("bin", StringComparison.OrdinalIgnoreCase)
                || Path.GetFileName(x).Equals("obj", StringComparison.OrdinalIgnoreCase)
                || Path.GetFileName(x).Equals("packages", StringComparison.OrdinalIgnoreCase)
            );
        }
        else
        {
            objBinDirectoryList = objBinDirectoryList.Where(x =>
                Path.GetFileName(x).Equals("bin", StringComparison.OrdinalIgnoreCase)
                || Path.GetFileName(x).Equals("obj", StringComparison.OrdinalIgnoreCase)
            );
        }

        Parallel.ForEach(
            objBinDirectoryList,
            new ParallelOptions { MaxDegreeOfParallelism = 4 },
            (dir) =>
            {
                if (printDeleted)
                    Console.WriteLine(dir);

                Directory.Delete(dir, true);
            }
        );

        Console.WriteLine(
            $"Deleted obj and bin directories for {baseDirectory} in {sw.Elapsed.TotalSeconds} seconds."
        );
    }
}
