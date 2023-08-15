using CaasId.src.Domain.Entities;
using Newtonsoft.Json;

namespace CaasId.src.Infrastructure.Adapters
{
	public static class JsonAdapter
	{
		public static List<Printer> CreateSigmaStock(List<Printer> sigmaList)
		{
			string json = JsonConvert.SerializeObject(sigmaList);
			string path = Default.Path;
			string fileName = Default.FileNameStock;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			File.WriteAllText(string.Concat(path, fileName), json);
			return sigmaList;
		}

		public static List<Printer>? GetPrinterList() {
			if (File.Exists(Default.PathStock))
			{
				using (StreamReader jsonStream = File.OpenText(Default.PathStock))
				{
					var json = jsonStream.ReadToEnd();
					return JsonConvert.DeserializeObject<List<Printer>>(json);
				}

			}
			else { 
				return null;
			}
		}
		public static List<PrinterConfig>? GetConfig()
		{
			if (File.Exists(Default.PathConfig))
			{
				using (StreamReader jsonStream = File.OpenText(Default.PathConfig))
				{
					var json = jsonStream.ReadToEnd();
					return JsonConvert.DeserializeObject<List<PrinterConfig>>(json);
				}
			}
			else
			{
				PrinterConfig defaultconfig = new(Default.ClientId, Default.OfficeId, Default.DocumentType, Default.FinderTime.ToString(), Default.PollingTime.ToString());
				List<PrinterConfig> lst = new List<PrinterConfig>();
				lst.Add(defaultconfig);
				return lst;
			}
		}
		public static int GetFinderTime => Convert.ToInt32(GetConfig().First().FindPrinterTime) > 0
				? Convert.ToInt32(GetConfig().First().FindPrinterTime) : Default.FinderTime;
		public static int GetPollingTime => Convert.ToInt32(GetConfig().First().PollingTime) > 0
				? Convert.ToInt32(GetConfig().First().PollingTime) : Default.PollingTime;
		public static string GetClientId => GetConfig().First().ClientId != null
				? GetConfig().First().ClientId : Default.ClientId;
		public static string GetOfficeId => GetConfig().First().OfficeId != null
				? GetConfig().First().OfficeId : Default.OfficeId;
	}
}
