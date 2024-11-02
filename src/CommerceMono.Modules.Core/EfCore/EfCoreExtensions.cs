using System.Linq.Expressions;
using CommerceMono.Modules.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace CommerceMono.Modules.Core.EfCore;

public static class EfCoreExtensions
{
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
}
