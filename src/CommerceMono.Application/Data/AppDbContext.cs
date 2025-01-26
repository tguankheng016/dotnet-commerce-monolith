using System.Data;
using System.Reflection;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.Domain;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Persistences;
using CommerceMono.Modules.Core.Sessions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace CommerceMono.Application.Data;

public class AppDbContext : IdentityDbContext<User, Role, long,
	UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, IDbContext
{
	private readonly ILogger<AppDbContext>? _logger;
	private IDbContextTransaction? _currentTransaction;
	private readonly IAppSession? _appSession;

	public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext>? logger = null, IAppSession? appSession = null) : base(options)
	{
		_logger = logger;
		_appSession = appSession;
	}

	public DbSet<UserRolePermission> UserRolePermissions => Set<UserRolePermission>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		base.OnModelCreating(builder);
		builder.SetSoftDeletedFilter();
		builder.ToSnakeCaseTableNames();
	}

	public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
	{
		if (_currentTransaction is not null) return;

		_currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await SaveChangesAsync(cancellationToken);
			await _currentTransaction?.CommitAsync(cancellationToken)!;
		}
		catch
		{
			await RollbackTransactionAsync(cancellationToken);
			throw;
		}
		finally
		{
			_currentTransaction?.Dispose();
			_currentTransaction = null;
		}
	}

	public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _currentTransaction?.RollbackAsync(cancellationToken)!;
		}
		finally
		{
			_currentTransaction?.Dispose();
			_currentTransaction = null;
		}
	}

	public IExecutionStrategy CreateExecutionStrategy()
	{
		return Database.CreateExecutionStrategy();
	}

	public bool HasActiveTransaction => _currentTransaction is not null;

	public Task ExecuteTransactionalAsync(CancellationToken cancellationToken = default)
	{
		var strategy = CreateExecutionStrategy();
		return strategy.ExecuteAsync(async () =>
		{
			await using var transaction =
				await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
			try
			{
				await SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
			catch
			{
				await transaction.RollbackAsync(cancellationToken);
				throw;
			}
		});
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		OnBeforeSaving();

		try
		{
			return await base.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateConcurrencyException ex)
		{
			foreach (var entry in ex.Entries)
			{
				var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

				if (databaseValues is null)
				{
					_logger?.LogError("The record no longer exists in the database, The record has been deleted by another user.");
					throw;
				}

				// Refresh the original values to bypass next concurrency check
				entry.OriginalValues.SetValues(databaseValues);
			}

			return await base.SaveChangesAsync(cancellationToken);
		}
	}

	private void OnBeforeSaving()
	{
		var userId = _appSession?.UserId ?? null;

		var entries = ChangeTracker.Entries();

		foreach (var entry in entries)
		{
			if (entry.Entity is ICreationAudited auditable)
			{
				switch (entry.State)
				{
					case EntityState.Added:
						auditable.CreatorUserId = userId;
						auditable.CreationTime = DateTimeOffset.Now;
						break;
					case EntityState.Modified:
						auditable.LastModifierUserId = userId;
						auditable.LastModificationTime = DateTimeOffset.Now;
						break;
				}
			}

			if (entry.Entity is ISoftDelete softDeletable)
			{
				switch (entry.State)
				{
					case EntityState.Deleted:
						entry.State = EntityState.Modified;
						softDeletable.DeleterUserId = userId;
						softDeletable.IsDeleted = true;
						softDeletable.DeletionTime = DateTimeOffset.Now;
						break;
				}
			}

			if (entry.Entity is IVersion versionable)
			{
				switch (entry.State)
				{
					case EntityState.Modified:
					case EntityState.Deleted:
						versionable.Version++;
						break;
				}
			}
		}
	}
}
