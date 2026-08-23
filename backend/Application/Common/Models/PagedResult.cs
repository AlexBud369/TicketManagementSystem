namespace Application.Common.Models;

public sealed class PagedResult<T> {
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public static class PagedResult {
    public static PagedResult<T> Create<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize) {
        return new PagedResult<T> {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public static PagedResult<T> Empty<T>(int page, int pageSize) {
        return new PagedResult<T> {
            Items = Enumerable.Empty<T>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }
}

