namespace CaasId.src.Domain.Entities
{
	public record PrinterDataState(
			DateTime fecha,
			string clientId,
			string officeId,
			string printerName,
			string printerModel,
			string printerSerialNumber,
			PrinterStatus PrinterStatus,
			int ErrorCode,
			int PrinterJobID,
			int TotalCompleted,
			int TotalPicked,
			int TotalLost,
			int TotalRejected,
			int indentRibbonRemaining,
			int laminatorL1PercentRemaining,
			int laminatorL2PercentRemaining,
			int ribbonRemaining,
			int retransferFilmRemaining,
			int tactileImpresserFoilRemaining,
			int topperRibbonRemaining
		);
}
