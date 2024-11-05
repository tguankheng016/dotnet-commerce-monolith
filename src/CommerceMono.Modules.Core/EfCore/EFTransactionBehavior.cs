using System.Text.Json;
using System.Text.Json.Serialization;
using CommerceMono.Modules.Core.Persistences;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceMono.Modules.Core.EFCore;

public class EFTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull, IRequest<TResponse>
	where TResponse : notnull
{
	private readonly ILogger<EFTransactionBehavior<TRequest, TResponse>> _logger;
	private readonly IDbContext _dbContext;

	public EFTransactionBehavior(
		ILogger<EFTransactionBehavior<TRequest, TResponse>> logger,
		IDbContext dbContext)
	{
		_logger = logger;
		_dbContext = dbContext;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// Apply for command handler only
		if (request is not ITransactional || request is INonTransactional)
		{
			return await next();
		}

		if (_dbContext.HasActiveTransaction)
		{
			return await next();
		}

		var prefix = nameof(EFTransactionBehavior<TRequest, TResponse>);
		var requestFullName = typeof(TRequest).FullName;

		_logger.LogInformation($"[{prefix}] Handled command {requestFullName}");

		var options = new JsonSerializerOptions
		{
			ReferenceHandler = ReferenceHandler.Preserve
		};

		_logger.LogDebug(
			$"[{prefix}] Handled command {requestFullName} with content {JsonSerializer.Serialize(request, options)}");

		_logger.LogInformation($"[{prefix}] Open the transaction for {requestFullName}");

		var strategy = _dbContext.CreateExecutionStrategy();

		return await strategy.ExecuteAsync(async () =>
		{
			await _dbContext.BeginTransactionAsync(cancellationToken);

			try
			{
				var response = await next();

				_logger.LogInformation(
					$"[{prefix}] Executed the {requestFullName} request");

				await _dbContext.CommitTransactionAsync(cancellationToken);

				return response;
			}
			catch
			{
				await _dbContext.RollbackTransactionAsync(cancellationToken);
				throw;
			}
		});
	}
}
