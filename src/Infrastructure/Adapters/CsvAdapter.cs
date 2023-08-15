using CaasId.src.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CaasId.src.Infrastructure.Adapters
{
	public static class CsvAdapter
	{
		public static bool RecordPrint(PrinterDataState printRecord)
		{
			var fileExistsil = true;
			if (!File.Exists(Default.PathRecord))
			{
				TextWriter file = new StreamWriter(Default.PathRecord);
				file.Close();
				fileExistsil = false;
			}

			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				Delimiter = ";",
				HasHeaderRecord = false,
				Encoding = Encoding.UTF8
			};
			try
			{
                using (var stream = File.Open(Default.PathRecord, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, config))
                {
                    if (!fileExistsil)
                    {
                        csv.WriteHeader<PrinterDataState>();
                        csv.NextRecord();
                    }
                    csv.WriteRecord(printRecord);
                    csv.NextRecord();
                }
            }
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return false;
			}		
			return true;	
		}

		public static bool RecordApiError(PrinterDataState printData, string? description)
		{
			if (description != null)
			{
				WindowsIdentity identidad = WindowsIdentity.GetCurrent();
				DataError printRecord = new(DateTime.Now, printData.ErrorCode, description, "Check Printer API", printData.printerName, identidad.Name);
				var fileExistsil = true;
				if (!File.Exists(Default.PathRecordError))
				{
					TextWriter file = new StreamWriter(Default.PathRecordError);
					file.Close();
					fileExistsil = false;
				}

				var config = new CsvConfiguration(CultureInfo.InvariantCulture)
				{
					Delimiter = ";",
					HasHeaderRecord = false,
					Encoding = Encoding.UTF8
				};
				try
				{
					using (var stream = File.Open(Default.PathRecordError, FileMode.Append))
					using (var writer = new StreamWriter(stream))
					using (var csv = new CsvWriter(writer, config))
					{
						if (!fileExistsil)
						{
							csv.WriteHeader<DataError>();
							csv.NextRecord();
						}
						csv.WriteRecord(printRecord);
						csv.NextRecord();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					return false;
				}
			}
			return true;
		}

	}
}
