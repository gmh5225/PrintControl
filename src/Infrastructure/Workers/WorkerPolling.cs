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
			Dictionary<string, PrinterDataState> printerData = new Dictionary<string, PrinterDataState>();
            PrinterDataState initDataState = new(DateTime.Now, "", "", "", "", "", PrinterStatus.Unknown, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            var sigmaList = JsonAdapter.CreateSigmaStock(SigmaPrinters.GetList());
            sigmaList.ForEach(printer =>
            {
                printerData.Add(printer.Name, initDataState);
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("STATUS IMPRESORA: {time}\n", DateTimeOffset.Now);
                Parallel.ForEach(sigmaList, async (print) =>
                {
                    Console.WriteLine(print);
                    printerData[print.Name] = SigmaPrinters.GetDataState(printerData[print.Name], print.Name, clientId, officeId);
                    CsvAdapter.RecordError(printerData[print.Name], SigmaPrinters.CheckError(printerData[print.Name]));
                    if (printerData[print.Name].PrinterStatus == PrinterStatus.PrintFinished)
                    {
                        while(!CsvAdapter.RecordPrint(printerData[print.Name]))
                            await Task.Delay(1000);
						Console.WriteLine("IMPRIMIO");
					}
                });
                await Task.Delay(delayWorker, stoppingToken);
            }
        }
    }
}