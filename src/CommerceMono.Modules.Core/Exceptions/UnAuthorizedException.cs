using Microsoft.AspNetCore.Http;

namespace CommerceMono.Modules.Core.Exceptions;

public class UnAuthorizedException : CustomException
{
	public UnAuthorizedException(string message, int? code = StatusCodes.Status401Unauthorized)
		: base(message, code: code)
	{
	}
}
