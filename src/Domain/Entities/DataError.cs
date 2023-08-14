using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasId.src.Domain.Entities
{
	public record DataError(
			DateTime fecha,
			int codeError,
			string description,
			string api,
			string printerName,
			string user
		);
}
