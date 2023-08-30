using CaasId.src.Infrastructure.Workers;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;

const string ServiceName = ".NET Joke Service";

if (args is { Length: 1 })
{
	try
	{
		string executablePath =
			Path.Combine(AppContext.BaseDirectory, "CaasId.exe");

		if (args[0] is "/Install")
		{
			await Cli.Wrap("sc")
				.WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
				.ExecuteAsync();
		}
		else if (args[0] is "/Uninstall")
		{
			await Cli.Wrap("sc")
				.WithArguments(new[] { "stop", ServiceName })
				.ExecuteAsync();

			await Cli.Wrap("sc")
				.WithArguments(new[] { "delete", ServiceName })
				.ExecuteAsync();
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex);
	}

	return;
}


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
	options.ServiceName = ".NET Joke Service";
});

LoggerProviderOptions.RegisterProviderOptions<
	EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddHostedService<WorkerFindPrinter>();
builder.Services.AddHostedService<WorkerPolling>();

builder.Logging.AddConfiguration(
	builder.Configuration.GetSection("Logging"));

IHost host = builder.Build();
host.Run();

/*IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services =>
	{
		services.AddHostedService<WorkerFindPrinter>();
		services.AddHostedService<WorkerPolling>();
	})
	.Build();

host.Run();*/
