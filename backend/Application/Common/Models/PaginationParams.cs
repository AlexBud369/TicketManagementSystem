namespace Application.Common.Models;

public sealed class PaginationParams {
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int MinPage = 1;

    private int _page = MinPage;
    private int _pageSize = DefaultPageSize;

    public int Page {
        get => _page;
        init => _page = value < MinPage ? MinPage : value;
    }

    public int PageSize {
        get => _pageSize;
        init => _pageSize = value switch {
            <= 0 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public int Skip => (Page - 1) * PageSize;

    public static PaginationParams Default => new();
}
