using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace VSUtilsExtension
{
    internal sealed class ViewInTFSCommand
    {
        public const int CommandId = 0x0101;

        public static readonly Guid CommandSet = new Guid("7bb3cbdf-a82a-46ff-a7a8-603a994041e5");

        private readonly AsyncPackage package;

        private ViewInTFSCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService =
                commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ViewInTFSCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ViewInTFSCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;

                var filePath = dte.ActiveDocument?.FullName;

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return;
                }

                var workingDirectory = Path.GetDirectoryName(filePath);

                var remoteUrl = RunGitCommandAndReadOutput(
                    workingDirectory,
                    "remote get-url origin"
                );
                var baseRepositoryPath = RunGitCommandAndReadOutput(
                    workingDirectory,
                    "rev-parse --show-toplevel"
                );

                if (string.IsNullOrEmpty(remoteUrl) || string.IsNullOrEmpty(baseRepositoryPath))
                {
                    ShowErrorDialog(
                        "Error",
                        "File is not part of a git repository or remote is not available."
                    );
                    return;
                }

                // Would be cool to use alongside &version=GB{currentBranch} but takes too much time, bad ux
                // var currentBranch = RunGitCommand(workingDirectory, "rev-parse --abbrev-ref HEAD").StandardOutput.ReadToEnd().Trim();
                // var branchExistsInRemote = RunGitCommand(workingDirectory, $"ls-remote --heads origin {currentBranch}").StandardOutput.ReadToEnd().Trim();

                var baseRepositoryDirectory = Path.GetFullPath(baseRepositoryPath);

                var remainingPath = filePath.TrimPrefix(baseRepositoryDirectory).Replace('\\', '/');

                var url = $"{remoteUrl}?path={remainingPath}";

                OpenInBrowser(url);
            }
            catch (Exception ex)
            {
                ShowErrorDialog("An Error Occurred", ex.Message);
            }
        }

        private void ShowErrorDialog(string title, string message)
        {
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
            );
        }

        private Process OpenInBrowser(string url)
        {
            return Process.Start("explorer.exe", $"\"{url}\"");
        }

        private string RunGitCommandAndReadOutput(string workingDirectory, string arguments)
        {
            var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                }
            );

            using (process)
            {
                return process.StandardOutput.ReadToEnd().Trim();
            }
        }
    }
}
