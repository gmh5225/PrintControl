using CaasId.src.Aplication.UseCases;
using CaasId.src.Domain.Entities;
using CaasId.src.Infrastructure.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CaasId.src.Infrastructure.Workers
{
	internal class InitWorker
	{

		public static Dictionary<string, PrinterDataState>? upDatePrintData(List<Printer>? sigmaList, Dictionary<string, PrinterDataState>? printerData)
		{
			PrinterDataState initDataState = new(DateTime.Now, "", "", "", "", "", PrinterStatus.Unknown, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			if(sigmaList != null && sigmaList.Count > 0 )
			{
				List<string> keys = printerData.Keys.ToList();
				var names = sigmaList.Select(x => x.Name).ToList();
				keys.ForEach(key =>
				{
					if(!names.Contains(key))
						printerData.Remove(key);
				});
				sigmaList.ForEach(printer =>
				{
					if(!printerData.ContainsKey(printer.Name))
						printerData.Add(printer.Name, initDataState);
				});
				return printerData;
			}
			return null;
		}

		public static List<Printer>? checkPrinterList(List<Printer> sigmalist) => (sigmalist == JsonAdapter.GetPrinterList() || JsonAdapter.GetPrinterList() != null)
				? sigmalist : JsonAdapter.GetPrinterList();

	}
}
