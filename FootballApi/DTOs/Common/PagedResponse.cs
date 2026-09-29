namespace FootballApi.DTOs.Common;

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];

    public PaginationMetadata Pagination { get; init; } = null!;
}
