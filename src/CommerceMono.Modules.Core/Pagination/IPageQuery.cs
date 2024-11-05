using CommerceMono.Modules.Core.CQRS;

namespace CommerceMono.Modules.Core.Pagination;

public interface IPageQuery<out TResponse> : IPageRequest, IQuery<TResponse>
    where TResponse : notnull
{
}
