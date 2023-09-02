using CaasId.src.Domain.Entities;
using CliWrap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasId.src.Aplication.UseCases
{
	public static class AppHelper
	{
		const string ServiceName = ".NET CaasId Service";
		public static bool initAppAsync(String[] args)
		{
			PrinterConfig defaultconfig = new(args[1], args[2], args[3], args[5], args[6]);
			List<PrinterConfig> lst = new()
			{
				defaultconfig
			};
			string json = JsonConvert.SerializeObject(lst);
			DirectoryInfo di = Directory.CreateDirectory(Default.Path);
			di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			File.WriteAllText(string.Concat(Default.Path, Default.FileNameConfig), json);
			return true;
		}
	}
}
