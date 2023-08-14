using CaasId.src.Domain.Core;
using CaasId.src.Domain.Entities;

namespace CaasId.src.Aplication.UseCases
{
	public static class SigmaPrinters
	{
		private static PrinterStatus statusToInt(string status) => status switch
		{
			"Ready" => PrinterStatus.Ready,
			"Unavailable" => PrinterStatus.Unavailable,
			"Busy" => PrinterStatus.Busy,
			"Error" => PrinterStatus.Error,
				_ => PrinterStatus.Unknown,
		};
		private static PrinterStatus analyzeStatus(PrinterStatus statusnew, PrinterStatus statusold, int jobId, int oldcounter, int newcounter)
		{ 
			if(statusnew == PrinterStatus.Busy && jobId != 0)
				return PrinterStatus.Printing;
			if (statusnew == PrinterStatus.Ready && statusold == PrinterStatus.Printing && newcounter > oldcounter)
				return PrinterStatus.PrintFinished;
			return statusnew;
		}
		public static List<Printer> GetList()
		{
			var list = new List<Printer>();
			ListPrinters.GetAllPrinters().ForEach(printer =>
			{
				if (Sdk.PrinterValidation(printer))
					list.Add(Sdk.GetModelPrinter(printer));
			});
			return list;
		}	
		public static PrinterDataState GetDataState(PrinterDataState oldDataState, string printerName, string clientId, string officeId)
		{
			var dataState = Sdk.GetStatePrinter(printerName);
			var status = analyzeStatus(statusToInt(dataState.Counters._printerStatus), oldDataState.PrinterStatus, dataState.State._printerJobID, 
				oldDataState.TotalCompleted, dataState.Counters._totalCompleted);
			PrinterDataState newDataState = new(DateTime.Now, clientId, officeId, printerName, dataState.Options._printerModel, dataState.Options._printerSerialNumber, 
				status, dataState.State._errorCode, dataState.State._printerJobID, dataState.Counters._totalCompleted, dataState.Counters._totalPicked, 
				dataState.Counters._totalLost, dataState.Counters._totalRejected, dataState.Supplies._indentRibbonRemaining, dataState.Supplies._laminatorL1PercentRemaining, 
				dataState.Supplies._laminatorL2PercentRemaining, dataState.Supplies._ribbonRemaining, dataState.Supplies._retransferFilmRemaining, 
				dataState.Supplies._tactileImpresserFoilRemaining, dataState.Supplies._topperRibbonRemaining);
			return newDataState;
		}

		public static string? CheckError(PrinterDataState dataState)
		{
			if (dataState.PrinterStatus == PrinterStatus.Unavailable)
				return "Doesn't answer from printer";
			return null;
		}

	}
}
