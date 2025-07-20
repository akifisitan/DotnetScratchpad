namespace Scratchpad.Lib.Abstractions;

public interface IProjectSharer
{
    Task Export(
        string basePath,
        string outputFilePath,
        CancellationToken cancellationToken = default
    );
    Task Import(
        string textFilePath,
        string outputDirectoryPath,
        CancellationToken cancellationToken = default
    );
}
