namespace CommerceMono.Modules.Core.Pagination;

public class ListResultDto<T>
{
    private IReadOnlyList<T>? _items;

    public IReadOnlyList<T> Items
    {
        get => _items ??= new List<T>();
        set => _items = value;
    }

    public ListResultDto()
    {
    }

    public ListResultDto(IReadOnlyList<T> items) => Items = items;
}
