using CaasId.src.Infrastructure.Workers;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;
using System.IO;
using System.Reflection.Metadata;
using CaasId.src.Domain.Entities;
using Newtonsoft.Json;

const string ServiceName = ".NET CaasId Service";

if (args.Length >= 1)
{
	try
	{
		string executablePath =
			Path.Combine(AppContext.BaseDirectory, "CaasId.exe");

		if (args[0] is "/Install")
		{
			PrinterConfig defaultconfig = new(args[1], args[2], args[3], args[5], args[6]);
			List<PrinterConfig> lst = new()
			{
				defaultconfig
			};
			string json = JsonConvert.SerializeObject(lst);
			await Cli.Wrap("sc")
				.WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
				.ExecuteAsync();
			File.WriteAllText(string.Concat(@"C:\CaasId\", "clientconfig.json"), json);
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

