namespace Scratchpad.Lib.FileSearch;

internal interface IFileSearcher
{
    Task Search(CancellationToken cancellationToken);
}
