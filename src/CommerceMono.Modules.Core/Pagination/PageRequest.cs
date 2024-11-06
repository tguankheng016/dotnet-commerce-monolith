namespace CommerceMono.Modules.Core.Pagination;

public class PageRequest : IPageRequest
{
    public int? SkipCount { get; init; } = 0;

    public int? MaxResultCount { get; init; } = 10;

    public string? Filters { get; init; }

    public string? Sorting { get; init; }

    public void Deconstruct(out int? skipCount, out int? maxResultCount, out string? filters, out string? sorting) =>
        (skipCount, maxResultCount, filters, sorting) = (SkipCount, MaxResultCount, Filters, Sorting);
}
