using CaasId.src.Aplication.UseCases;
using CaasId.src.Infrastructure.Adapters;

namespace CaasId.src.Infrastructure.Workers
{
	public class WorkerFindPrinter : BackgroundService
	{
		private readonly ILogger<WorkerFindPrinter> _logger;
		public WorkerFindPrinter(ILogger<WorkerFindPrinter> logger)
		{
			_logger = logger;
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var delayWorker = JsonAdapter.GetFinderTime;
			var sigmaListold = JsonAdapter.CreateSigmaStock(SigmaPrinters.GetList());
			var sigmaListnew = sigmaListold;
			while (!stoppingToken.IsCancellationRequested)
			{
				sigmaListnew = SigmaPrinters.GetList();
				if (!sigmaListnew.SequenceEqual(sigmaListold))
					sigmaListold = JsonAdapter.CreateSigmaStock(SigmaPrinters.GetList());
				await Task.Delay(delayWorker, stoppingToken);
			}
		}
	}
}
