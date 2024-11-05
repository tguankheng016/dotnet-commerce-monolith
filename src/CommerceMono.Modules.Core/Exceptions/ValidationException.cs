namespace CommerceMono.Modules.Core.Exceptions;

public class ValidationException : CustomException
{
    public ValidationException(string message, int? code = null) : base(message, code: code)
    {
    }
}