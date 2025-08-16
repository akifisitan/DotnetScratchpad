using Microsoft.Extensions.DependencyInjection;
using Sharprompt;

//using Spectre.Console;

var services = new ServiceCollection();

var serviceProvider = services.BuildServiceProvider(validateScopes: true);

//var service = serviceProvider.GetRequiredService<Program>();

var city = Prompt.Select(
    "Select your city",
    ["Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai"],
    pageSize: 3,
    textInputFilter: (item, keyword) => item.Contains(keyword, StringComparison.OrdinalIgnoreCase)
);
Console.WriteLine($"Hello, {city}!");

List<string> strings =
[
    "MySrv",
    "YourSrv",
    "OurSrv123",
    "MySrv344",
    "YourSrv13232",
    "OurSrv125563",
    "MySrv7767",
    "YourSrv3254324",
    "OurSrv12334bbc",
    "OurSrv12334bbd",
    "OurSrv12334bbf",
    "OurSrv12334bbv",
    "OurSrv12334bb23b",
    "OurSrv12334bbn",
    "OurSrv12334bb213",
    "OurSrv12334bb23cad",
    "OurSrv12334bb12",
    "OurSrv12334bbdwa",
    "OurSrv12334bbadcaz",
    "OurSrv12334bb213",
    "OurSrv12334bbdwa",
];

//var result = AnsiConsole.Prompt(
//    new SelectionPrompt<string>()
//        .Title("Select search time")
//        .AddChoices(["Last 1 hour", "Last 2 hours", "Last 3 hours", "Custom"])
//        .AddChoices(strings)
//        .MoreChoicesText("123123")
//        .PageSize(3)
//        .WrapAround()
//        .EnableSearch()
//        .UseConverter(x => $"WOWZER {x}")
//);

//Console.WriteLine($"Selected choice: {result}.");

//if (result == "Custom")
//{
//    var a = AnsiConsole.Prompt(new TextPrompt<DateTime>("Enter a datetime."));
//    Console.WriteLine(a);
//}
