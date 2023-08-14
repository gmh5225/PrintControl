using CaasId.src.Infrastructure.Workers;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices(services =>
	{
		services.AddHostedService<WorkerFindPrinter>();
		services.AddHostedService<WorkerPolling>();
	})
	.Build();

host.Run();
