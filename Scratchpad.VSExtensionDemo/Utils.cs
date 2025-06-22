using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSExtensionDemo
{
    internal static class Utils
    {
        private static void ShowErrorDialog(AsyncPackage package, string title, string message)
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

        private static void ShowDialog(AsyncPackage package, string title, string message)
        {
            VsShellUtilities.ShowMessageBox(
                package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
            );
        }
    }
}
