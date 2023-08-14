namespace CaasId.src.Domain.Entities
{
	public record PrinterConfig(
		string ClientId,
		string OfficeId,
		string DocumentType,
		string FindPrinterTime,
		string PollingTime
		);
}
