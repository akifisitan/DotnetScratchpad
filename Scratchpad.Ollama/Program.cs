using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder();

const string model = "llama3.2";

//const string model = "deepseek-r1:1.5b";


builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
});

var ollamaChatClient = new OllamaChatClient(new Uri("http://localhost:11434"), model)
    .AsBuilder()
    .UseFunctionInvocation()
    //.UseLogging()
    .Build();

builder.Services.AddChatClient(ollamaChatClient);

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();

List<ChatMessage> messages =
[
    //new ChatMessage(
    //    ChatRole.System,
    //    """
    //    You are a helpful assistant. You have a single tool available to you: GetWeather. You use the tools available to you only when necessary.
    //    """
    //),
];

[Description("Gets the weather")]
static string GetWeather() =>
    Random.Shared.NextDouble() > 0.5
        ? "It's sunny and ... (complete it yourself)"
        : "It's raining and ... (complete it yourself)";

//static string GetWeather() => "Its dark and haily outside.";

var chatOptions = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(GetWeather)],
    //ResponseFormat = ChatResponseFormat.Text
};

var cts = new CancellationTokenSource();

while (true)
{
    Console.Write("Enter your prompt: ");
    var userMessage = Console.ReadLine()!;
    messages.Add(new ChatMessage(ChatRole.User, userMessage));

    var assistantResponseBuilder = new StringBuilder();

    var responseStream = chatClient.GetStreamingResponseAsync(messages, chatOptions, cts.Token);

    await foreach (var responseUpdate in responseStream)
    {
        Console.Write(responseUpdate.Text);
        assistantResponseBuilder.Append(responseUpdate.Text);
    }

    messages.Add(new ChatMessage(ChatRole.Assistant, assistantResponseBuilder.ToString()));
    Console.WriteLine();
}
