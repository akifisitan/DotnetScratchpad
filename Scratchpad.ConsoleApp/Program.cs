using ConsoleAppFramework;
using Scratchpad.Lib;

internal class Program
{
    private static void Main(string[] args)
    {
        //if (args.Length == 0)
        //{
        //    Console.WriteLine("No args provided. Press any key to exit.");
        //    Console.ReadKey(true);
        //    Environment.Exit(1);
        //}

        var app = ConsoleApp.Create();

        app.Add<Commands>();

        app.Run(args);
    }

    public class Commands
    {
        /// <summary>
        /// Search zipped log files.
        /// </summary>
        /// <param name="searchPattern">Search pattern (regex)</param>
        /// <param name="searchDirectory">Path to the zip file to search files in</param>
        /// <param name="startDate">-after, Start date</param>
        /// <param name="endDate">-before, End date</param>
        [Command("zip")]
        public void Zip(
            [Argument] string searchPattern,
            [Argument] string searchDirectory,
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null
        )
        {
            ZipSearcher.SearchLogFiles(searchPattern, searchDirectory, startDate, endDate);
        }

        /// <summary>
        /// Search unzipped log files.
        /// </summary>
        /// <param name="searchPattern">Search pattern (regex)</param>
        /// <param name="searchDirectory">Path of the directory to search files in</param>
        /// <param name="startDate">-after, Start date</param>
        /// <param name="endDate">-before, End date</param>
        [Command("file")]
        public void File(
            [Argument] string searchPattern,
            [Argument] string searchDirectory,
            DateTimeOffset? startDate = null,
            DateTimeOffset? endDate = null
        )
        {
            FileSearcher.SearchLogFiles(searchPattern, searchDirectory, startDate, endDate);
        }
    }
}
