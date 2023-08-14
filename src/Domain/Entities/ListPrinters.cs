using System.Drawing.Printing;

namespace CaasId.src.Domain.Entities
{
	public static class ListPrinters
	{
		public static List<string> GetAllPrinters()
		{
			List<string> lst = new List<string>();
			for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
			{
				lst.Add(PrinterSettings.InstalledPrinters[i]);
			}
			return lst;
		}

	}
}
