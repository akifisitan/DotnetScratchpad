using LogSearch.ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<Service>();

var serviceProvider = services.BuildServiceProvider(validateScopes: true);

var service = serviceProvider.GetRequiredService<Service>();

service.Run();
