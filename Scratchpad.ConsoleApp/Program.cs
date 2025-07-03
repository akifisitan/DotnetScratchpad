using System.Globalization;
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
        /// Search log files.
        /// </summary>
        /// <param name="searchPattern">Search pattern (regex)</param>
        /// <param name="searchDirectory">Path of the directory to search files in</param>
        /// <param name="searchZip">-z, Search inside zipped files or not</param>
        /// <param name="include">Include glob patterns</param>
        /// <param name="exclude">Exclude glob patterns</param>
        /// <param name="startDateTime">-sdt, Start datetime</param>
        /// <param name="endDateTime">-edt, End datetime</param>
        /// <param name="startTime">-st, Start time in HH:mm:ss</param>
        /// <param name="endTime">-et, End time in HH:mm:ss</param>
        /// <param name="interactive">Interactive or not</param>
        [Command("file")]
        public void File(
            [Argument] string searchPattern,
            [Argument] string searchDirectory,
            bool searchZip = false,
            string include = "*",
            string? exclude = null,
            DateTimeOffset? startDateTime = null,
            DateTimeOffset? endDateTime = null,
            string? startTime = null,
            string? endTime = null,
            bool interactive = false
        )
        {
            var (s, isValid) = ValidateInput(startTime);

            Console.WriteLine(startDateTime);

            FileSearcher.SearchLogFiles(
                searchPattern,
                searchDirectory: searchDirectory,
                includePattern: include,
                excludePattern: exclude,
                startDate: startDateTime,
                endDate: endDateTime,
                searchZip: searchZip
            );
        }

        private static (DateTime a, bool IsValid) ValidateInput(string? dateTimeString)
        {
            DateTime res = DateTime.MinValue;

            if (
                dateTimeString is not null
                && !DateTime.TryParseExact(
                    dateTimeString,
                    "HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out res
                )
            )
            {
                Console.WriteLine(
                    $"Argument 'after-hour' failed to parse, provided value: {dateTimeString}. Expected format: HH:mm:ss"
                );
                return (default, false);
            }

            return (res, true);
        }
    }
}
