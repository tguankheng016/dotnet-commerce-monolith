using System.Linq.Expressions;
using CommerceMono.Modules.Core.Domain;
using CommerceMono.Modules.Core.Persistences;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Core.EFCore;

public static class EFCoreExtensions
{
	public static void ToSnakeCaseTableNames(this ModelBuilder modelBuilder)
	{
		foreach (var entity in modelBuilder.Model.GetEntityTypes())
		{
			// Replace table names
			entity.SetTableName(entity.GetTableName()?.Underscore());
		}
	}

	public static void SetSoftDeletedFilter(this ModelBuilder modelBuilder)
	{
		Expression<Func<ISoftDelete, bool>> filterExpr = e => !e.IsDeleted;

		foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes()
			.Where(m => m.ClrType.IsAssignableTo(typeof(ISoftDelete))))
		{
			// modify expression to handle correct child type
			var parameter = Expression.Parameter(mutableEntityType.ClrType);
			var body = ReplacingExpressionVisitor
				.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
			var lambdaExpression = Expression.Lambda(body, parameter);

			// set filter
			mutableEntityType.SetQueryFilter(lambdaExpression);
		}
	}

	public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app)
		where TContext : DbContext, IDbContext
	{
		MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();
		SeedDataAsync(app.ApplicationServices).GetAwaiter().GetResult();

		return app;
	}

	private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
		where TContext : DbContext, IDbContext
	{
		using var scope = serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<TContext>();
		await context.Database.MigrateAsync();
	}

	private static async Task SeedDataAsync(IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
		foreach (var seeder in seeders)
		{
			await seeder.SeedAllAsync();
		}
	}
}
