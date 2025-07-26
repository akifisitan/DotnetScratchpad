using ConsoleAppFramework;
using LogGrep;

var app = ConsoleApp
    .Create()
    .ConfigureServices(x =>
    {
        x.AddLogGrep();
    });

app.Add<AppCommands>();
app.UseFilter<MyFilter>();

app.Run(args);
