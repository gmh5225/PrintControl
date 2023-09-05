using CaasId.src.Aplication.UseCases;
using CaasId.src.Domain.Core;
using CaasId.src.Domain.Entities;
using CaasId.src.Infrastructure.Adapters;
using System.Collections.Generic;
using System.Reflection;

namespace CaasId.src.Infrastructure.Workers
{
    public class WorkerPolling : BackgroundService
    {
        private readonly ILogger<WorkerPolling> _logger;
        public WorkerPolling(ILogger<WorkerPolling> logger)
        {
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delayWorker = JsonAdapter.GetPollingTime;
            var clientId = JsonAdapter.GetClientId;
			var officeId = JsonAdapter.GetOfficeId;
			Dictionary<string, PrinterDataState>? printerData = new Dictionary<string, PrinterDataState>();
            List<Printer>? sigmaList = null;

			while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("STATUS IMPRESORA: {time}\n", DateTimeOffset.Now);
				sigmaList = JsonAdapter.GetPrinterList();
				printerData = InitWorker.upDatePrintData(sigmaList, printerData);
                Parallel.ForEach(sigmaList, async (print) =>
                {
                    Console.WriteLine(print);
					Console.WriteLine(printerData[print.Name]);
					printerData[print.Name] = SigmaPrinters.GetDataState(printerData[print.Name], print.Name, clientId, officeId);
                    CsvAdapter.RecordApiError(printerData[print.Name], SigmaPrinters.CheckError(printerData[print.Name]), _logger);
                    if ((printerData[print.Name].PrinterStatus == PrinterStatus.PrintFinished) || (printerData[print.Name].ErrorCode > 0))
                    {
                        while(!CsvAdapter.RecordPrint(printerData[print.Name]))
                            await Task.Delay(1000);
						_logger.LogWarning("Ingreso de Registro en Archivo");
					}
                });
                await Task.Delay(delayWorker, stoppingToken);
            }
        }
    }
}