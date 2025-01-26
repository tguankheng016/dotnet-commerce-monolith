namespace CommerceMono.Modules.Core.Pagination;

public class PageQuery<TResponse> : PageRequest, IPageQuery<TResponse>
	where TResponse : notnull;