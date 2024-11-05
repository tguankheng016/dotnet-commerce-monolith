namespace CommerceMono.Modules.Core.Pagination;

public interface IPageRequest
{
    int SkipCount { get; init; }

    int MaxResultCount { get; init; }

    string? Filters { get; init; }

    string? Sorting { get; init; }
}