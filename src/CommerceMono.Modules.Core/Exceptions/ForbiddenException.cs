
using Microsoft.AspNetCore.Http;

namespace CommerceMono.Modules.Core.Exceptions;

public class ForbiddenException : CustomException
{
	public ForbiddenException(string message, int? code = StatusCodes.Status403Forbidden)
		: base(message, code: code)
	{
	}
}
