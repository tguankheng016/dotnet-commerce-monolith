using FluentValidation;
using ValidationException = CommerceMono.Modules.Core.Exceptions.ValidationException;

namespace CommerceMono.Modules.Core.Validations;

public static class ValidationExtensions
{
	public static async Task HandleValidationAsync<TRequest>(this IValidator<TRequest> validator, TRequest request)
	{
		var validationResult = await validator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors?.First()?.ErrorMessage!);
		}
	}
}
