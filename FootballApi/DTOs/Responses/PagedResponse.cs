public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];

    public PaginationMetadata Pagination { get; init; } = null!;
}

public sealed class PaginationMetadata
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}