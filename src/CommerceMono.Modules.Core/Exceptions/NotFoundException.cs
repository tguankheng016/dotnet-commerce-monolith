using System;

namespace CommerceMono.Modules.Core.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message, int? code = null) : base(message, code: code)
    {
    }
}