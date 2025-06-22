using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Scratchpad.Lib;

public class Pipeline
{
    readonly record struct Project(string Name, string FileName, string Guid, string RelativePath);

    private static void TransformRepository(
        IEnumerable<string> csprojFilePathList,
        List<string> unusedProjects,
        string packagesDirectoryPath,
        string allProjectPath,
        List<Project> projectList
    )
    {
        foreach (var csprojFilePath in csprojFilePathList)
        {
            var projectFileName = Path.GetFileName(csprojFilePath);

            if (unusedProjects.Contains(projectFileName))
            {
                continue;
            }

            var projectRootPath = Path.GetDirectoryName(csprojFilePath)!;
            var csprojFileContent = File.ReadAllText(csprojFilePath);
            var relativePackageDirectoryPath = Path.GetRelativePath(
                projectRootPath,
                packagesDirectoryPath
            );

            var matchPackagePattern = @"(\.\.\\)+packages"; // regex for ..\packages or ..\..\packages or ..\..\..\packages or ...etc

            var replacedCsprojFileContent = Regex.Replace(
                csprojFileContent,
                matchPackagePattern,
                relativePackageDirectoryPath
            );

            var csprojXml = XDocument.Parse(replacedCsprojFileContent);

            var projectGuid = Guid.NewGuid().ToString();

            var hintPaths = csprojXml.Descendants().Where(x => x.Name.LocalName == "HintPath");

            var project = new Project(
                projectFileName[..projectFileName.LastIndexOf('.')],
                projectFileName,
                projectGuid,
                Path.GetRelativePath(allProjectPath, csprojFilePath)
            );

            projectList.Add(project);

            foreach (var hintPathNode in hintPaths)
            {
                // This step should be retired, these should be replaced with project references anyway
                const string debugPattern = @"\bin\Debug";
                const string releasePattern = @"\bin\Release";
                if (hintPathNode.Value.Contains(debugPattern, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"{projectFileName}: {hintPathNode.Value}");
                    hintPathNode.Value = hintPathNode.Value.Replace(
                        debugPattern,
                        releasePattern,
                        StringComparison.OrdinalIgnoreCase
                    );
                }

                // Replace relative package references with repository package store
                const string packagePattern = @"\packages\";
                var idx = hintPathNode.Value.IndexOf(packagePattern);
                if (idx != -1 && !hintPathNode.Value.StartsWith(relativePackageDirectoryPath))
                {
                    hintPathNode.Value =
                        @$"{relativePackageDirectoryPath}\{hintPathNode.Value[(idx + packagePattern.Length)..]}";
                    continue;
                }
            }

            csprojXml.Save(csprojFilePath);
        }
    }

    public static void Run(
        string basePath,
        string packagesDirectoryPath,
        string nugetConfigFilePath,
        bool restorePackages,
        bool cleanBinObj,
        bool cleanPackages,
        string slnFileName
    )
    {
        var sw = Stopwatch.StartNew();
        var sw2 = Stopwatch.StartNew();

        double timeTakenNugetRestore = -1;
        double timeTakenTotal = -1;
        double timeTakenBuild = -1;
        double timeTakenRepositoryTransform = -1;

        if (cleanBinObj)
        {
            PipelineUtils.CleanSolution(basePath, cleanPackages);
            Console.WriteLine($"Clean completed in {sw2.Elapsed.TotalSeconds} seconds.");
            sw2.Restart();
        }

        Console.WriteLine($"Listing projects in {basePath}");

        IEnumerable<string> csprojFilePathList = Directory.EnumerateFiles(
            basePath,
            "*.csproj",
            SearchOption.AllDirectories
        );

        var allProjectPath = Path.Combine(basePath, "AllProject");
        var projectList = new List<Project>();

        // Maybe read this part from a config file
        List<string> unusedProjects = [];

        TransformRepository(
            csprojFilePathList,
            unusedProjects,
            packagesDirectoryPath,
            allProjectPath,
            projectList
        );
        Console.WriteLine("Transformed repository successfully.");
        Environment.Exit(0);

        var slnPath = Path.Combine(allProjectPath, slnFileName);
        CreateSlnFile(projectList, slnPath);

        timeTakenRepositoryTransform = sw2.Elapsed.TotalSeconds;
        sw2.Restart();

        if (restorePackages)
        {
            PipelineUtils.NugetRestore(
                slnPath,
                basePath,
                nugetConfigFilePath,
                packagesDirectoryPath
            );
            timeTakenNugetRestore = sw2.Elapsed.TotalSeconds;
            sw2.Restart();
        }

        BuildSolution(
            slnPath,
            basePath,
            usedCores: 0,
            minimalVerbosity: false,
            logErrorsOnly: true
        );

        timeTakenBuild = sw2.Elapsed.TotalSeconds;
        timeTakenTotal = sw.Elapsed.TotalSeconds;

        Console.WriteLine($"Nuget restore completed in {timeTakenNugetRestore} seconds.");
        Console.WriteLine($"Repository transformed in {timeTakenRepositoryTransform} seconds.");
        Console.WriteLine($"Build completed in {timeTakenBuild} seconds.");
        Console.WriteLine($"\n-------\nTotal time taken: {timeTakenTotal} seconds.");
    }

    private static void CreateSlnFile(List<Project> projectList, string filePath)
    {
        var sb = new StringBuilder(
            """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.32014.148
            MinimumVisualStudioVersion = 10.0.40219.1

            """
        );

        const string projectTypeGuid = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";

        foreach (var project in projectList)
        {
            sb.AppendLine(
                $"Project(\"{{{projectTypeGuid}}}\") = \"{project.Name}\", \"{project.RelativePath}\", \"{project.Guid}\""
            );
            sb.AppendLine("EndProject");
        }

        sb.AppendLine("Global");
        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
        sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
        sb.AppendLine("\tEndGlobalSection");

        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

        foreach (var project in projectList)
        {
            sb.AppendLine($"\t\t{project.Guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            sb.AppendLine($"\t\t{project.Guid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            sb.AppendLine($"\t\t{project.Guid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            sb.AppendLine($"\t\t{project.Guid}.Release|Any CPU.Build.0 = Release|Any CPU");
        }

        sb.AppendLine("\tEndGlobalSection");
        sb.AppendLine("EndGlobal");

        File.WriteAllText(filePath, sb.ToString());
    }

    private static void BuildSolution(
        string buildPath,
        string workingDirectory,
        int usedCores = 0,
        bool minimalVerbosity = false,
        bool logErrorsOnly = false
    )
    {
        const string msbuildExePath = @"C:\Program Files (x86)\MSBuild\14.0\bin\msbuild.exe";

        var identifier = Path.GetFileName(buildPath);
        var logPath = Path.Combine(workingDirectory, "BuildLogs");
        var verbosity = minimalVerbosity ? "minimal" : "normal";

        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        var logFilePath = Path.Combine(logPath, $"{identifier}_BuildErrors.log");

        var commandBuilder = new StringBuilder(buildPath);

        commandBuilder.Append(" /nologo");
        commandBuilder.Append(" /nr:false");
        commandBuilder.Append(" /p:WarningLevel=0");
        commandBuilder.Append(" /p:platform=\"Any CPU\"");
        commandBuilder.Append(" /p:configuration=\"Release\"");
        commandBuilder.Append(" /p:VisualStudioVersion=\"14.0\"");

        commandBuilder.Append(usedCores == 0 ? " /m" : $" /m:{usedCores}");

        // /bl for understanding how its built -bl:{logfilePath}build.binlog
        commandBuilder.Append($" /fileLogger /flp:logfile={logFilePath};verbosity={verbosity}");

        if (logErrorsOnly)
        {
            commandBuilder.Append(";errorsonly");
        }

        using var process = PipelineUtils.StartProcessAndReadOutput(
            msbuildExePath,
            commandBuilder.ToString(),
            workingDirectory
        );

        process.WaitForExit();
    }
}
