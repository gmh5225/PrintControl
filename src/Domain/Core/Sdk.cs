
using CaasId.src.Domain.Core.SDKLibs;
using CaasId.src.Domain.Entities;

namespace CaasId.src.Domain.Core
{
    public static class Sdk
	{
		public static bool PrinterValidation(string printerName)
		{
			BidiSplWrap bidiSpl = null;
			try
			{
				bidiSpl = new BidiSplWrap();
				bidiSpl.BindDevice(printerName);

				string driverVersionXml = bidiSpl.GetPrinterData(strings.SDK_VERSION);
				Console.WriteLine(Environment.NewLine + "driver version: " + Util.ParseDriverVersionXML(driverVersionXml) + Environment.NewLine);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}
			finally
			{
				bidiSpl.UnbindDevice();
			}
		}

		public static Printer GetModelPrinter(string printerName)
		{
			BidiSplWrap bidiSpl = null;
			try
			{
				bidiSpl = new BidiSplWrap();
				bidiSpl.BindDevice(printerName);

				string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
				PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

				Printer printer = new(printerName, printerOptionsValues._printerModel, printerOptionsValues._printerSerialNumber);
				return printer;

			}
			catch (Exception e)
			{
				Printer printer = null;
				Console.WriteLine(e.Message);
				return printer;
			}
			finally
			{
				bidiSpl.UnbindDevice();
			}
		}

		public static (PrinterStatusValues? State, PrinterCounterStatus? Counters, SuppliesValues? Supplies, PrinterOptionsValues Options) GetStatePrinter(string printerName)
		{
			BidiSplWrap bidiSpl = null;
			try
			{
				bidiSpl = new BidiSplWrap();
				bidiSpl.BindDevice(printerName);

				string printerStatusXML = bidiSpl.GetPrinterData(strings.PRINTER_MESSAGES);
				PrinterStatusValues printerStatusValues = Util.ParsePrinterStatusXML(printerStatusXML);

				string printerCardCountXML = bidiSpl.GetPrinterData(strings.COUNTER_STATUS2);
				PrinterCounterStatus printerCounterStatusValues = Util.ParsePrinterCounterStatusXML(printerCardCountXML);

				string suppliesXML = bidiSpl.GetPrinterData(strings.SUPPLIES_STATUS3);
				SuppliesValues suppliesValues = Util.ParseSuppliesXML(suppliesXML);

				string printerOptionsXML = bidiSpl.GetPrinterData(strings.PRINTER_OPTIONS2);
				PrinterOptionsValues printerOptionsValues = Util.ParsePrinterOptionsXML(printerOptionsXML);

				return (printerStatusValues, printerCounterStatusValues, suppliesValues, printerOptionsValues);

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return (null, null, null, null);
			}
			finally
			{
				bidiSpl.UnbindDevice();
			}
		}

	}

}
