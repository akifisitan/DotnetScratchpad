namespace Scratchpad.Lib.Abstractions;

public interface IDirectoryPacker
{
    Task Pack(
        string basePath,
        string outputFilePath,
        CancellationToken cancellationToken = default
    );
    Task Unpack(
        string textFilePath,
        string outputDirectoryPath,
        CancellationToken cancellationToken = default
    );
}
