namespace RandaSports.Application.Common.Wrappers;

public sealed record Paged<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
