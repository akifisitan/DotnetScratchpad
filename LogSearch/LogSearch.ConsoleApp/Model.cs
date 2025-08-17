namespace LogSearch.ConsoleApp;

public sealed record ServiceInfo
{
    public required string ServiceName { get; set; }
    public required string LogPath { get; set; }
    public required string Identifier { get; set; }
    public required List<string> Servers { get; set; }
    public required string LogSearchPattern { get; set; }
}
