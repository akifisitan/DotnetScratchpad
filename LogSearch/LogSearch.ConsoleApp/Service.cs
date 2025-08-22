using System.ComponentModel.DataAnnotations;
using FuzzySharp;
using Vaerktojer.Prompt;


namespace LogSearch.ConsoleApp;

internal sealed class Service
{
    private readonly List<Func<object?, ValidationResult?>> regularValidators =
    [
        value =>
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult("Username must not be empty");
            }

            return null;
        },
    ];

    public void Run()
    {
        var (userName, password) = Login();

        var data = Load(userName, password);

        var applicationName = Prompt.Select(
            "Select application to search",
            data.Keys,
            pageSize: 2,
            textInputFilter: SearchFilter
        );

        var servers = Prompt.MultiSelect(
            "Select servers to search",
            data[applicationName].Servers,
            pageSize: 2,
            textInputFilter: SearchFilter
        );

        var (startDate, endDate) = GetDates();

        // Search here...

        Console.WriteLine("Found files");

        var foundFiles = new List<(string FilePath, DateTime LastModifiedDate)>(
            Enumerable
                .Range(1, 5)
                .Select(x =>
                    (
                        $@"C:\users\user\desktop\demos\hello{x}.log",
                        DateTime.Now.Subtract(TimeSpan.FromMinutes(x))
                    )
                )
        );

        var z = foundFiles.Select(x => x.FilePath).ToList();

        var selectedFoundFiles = Prompt.MultiSelect(
            "Select files to open",
            z,
            textInputFilter: SearchFilter
        );

        foreach (var foundFile in selectedFoundFiles)
        {
            // OpenWithNpp(foundFile)
        }
    }

    private (string Username, string Password) Login()
    {
        while (true)
        {
            var username = Prompt.Input<string>("Username", validators: regularValidators);
            var password = Prompt.Password("Password", validators: regularValidators);

            if (username == "admin")
            {
                Console.WriteLine("Logged in as admin.");
                return (username, password);
            }
            else
            {
                Console.WriteLine("Invalid credentials.");
            }
        }
    }

    private static (DateTime StartDate, DateTime EndDate) GetDates()
    {
        var searchIntervalChoice = Prompt.Select(
            "Select search interval",
            [
                15,
                60,
                60 * 2,
                60 * 3,
                60 * 6,
                60 * 12,
                60 * 24,
                60 * 24 * 2,
                60 * 24 * 5,
                60 * 24 * 7,
                -1,
            ],
            textSelector: value =>
            {
                return value switch
                {
                    <= 0 => "Custom",
                    < 60 => $"Last {value} minutes",
                    >= 60 * 24 * 7 => $"Last {value / (60 * 24)} weeks",
                    >= 60 * 24 => $"Last {value / (60 * 24)} days",
                    >= 60 => $"Last {value / 60} hours",
                };
            },
            searchIsEnabled: false
        );

        if (searchIntervalChoice != -1)
        {
            return (
                DateTime.Now.Subtract(TimeSpan.FromMinutes(searchIntervalChoice)),
                DateTime.Now.Subtract(TimeSpan.FromMinutes(1))
            );
        }

        var startDate = Prompt.Input<DateTime>(
            "Enter start date",
            placeholder: "YYYY-MM-dd HH:mm:ss format",
            defaultValue: DateTime.Now.Subtract(TimeSpan.FromMinutes(5)),
            validators:
            [
                value =>
                {
                    if (value is not DateTime date)
                    {
                        return new ValidationResult($"{value} is not a valid value for DateTime.");
                    }

                    if (date > DateTime.Now)
                    {
                        return new ValidationResult("Start date must be less than or equal to now");
                    }

                    if (date < DateTime.Now.Subtract(TimeSpan.FromDays(365 * 5)))
                    {
                        return new ValidationResult("Start date must be bigger than 5 years ago");
                    }

                    return null;
                },
            ]
        );

        var endDate = Prompt.Input<DateTime>(
            "Enter end date",
            placeholder: "YYYY-MM-dd HH:mm:ss format",
            validators:
            [
                value =>
                {
                    if (value as DateTime? == DateTime.MinValue)
                    {
                        return new ValidationResult("Value is required");
                    }

                    if (value as DateTime? <= startDate)
                    {
                        return new ValidationResult("End date must be bigger than start date");
                    }

                    return null;
                },
            ]
        );

        return (startDate, endDate);
    }

    private static bool SearchFilter(string item, string keyword)
    {
        return keyword.Length < 10
            ? item.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            : Fuzz.Ratio(item.ToUpperInvariant(), keyword.ToUpperInvariant()) > 60;
    }

    private static Dictionary<string, ServiceInfo> Load(string userName, string password)
    {
        Dictionary<string, ServiceInfo> v = [];

        foreach (var line in File.ReadAllLines("data.txt"))
        {
            var spl = line.Split(',');

            List<string> b = [];
            foreach (var item in spl[3].Split(';'))
            {
                var sp = item.Split('-');
                var first = sp[0];
                var num = sp.ElementAtOrDefault(1);

                if (num is null)
                {
                    b.Add(first);
                    continue;
                }

                var num1 = int.Parse(num);

                for (int i = 1; i < num1; i++)
                {
                    b.Add($"{first}{i}");
                }
            }

            var a = new ServiceInfo
            {
                ServiceName = spl[0],
                LogPath = spl[1],
                Identifier = spl[2],
                Servers = b,
                LogSearchPattern = spl[4],
            };

            v[a.ServiceName] = a;
        }

        return v;
    }
}
