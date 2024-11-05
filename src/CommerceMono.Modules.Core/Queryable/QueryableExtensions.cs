using System.Linq.Expressions;
using CommerceMono.Modules.Core.Pagination;

namespace CommerceMono.Modules.Core.Queryable;

public static class QueryableExtensions
{
    public static IQueryable<T> PageBy<T>(
        this IQueryable<T> query,
        int skipCount,
        int maxResultCount)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        return query.Skip<T>(skipCount).Take<T>(maxResultCount);
    }

    public static IQueryable<T> PageBy<T>(
        this IQueryable<T> query,
        IPageRequest pagedResultRequest)
    {
        return query.PageBy<T>(pagedResultRequest.SkipCount, pagedResultRequest.MaxResultCount);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return !condition ? query : query.Where<T>(predicate);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, int, bool>> predicate)
    {
        return !condition ? query : query.Where<T>(predicate);
    }
}
