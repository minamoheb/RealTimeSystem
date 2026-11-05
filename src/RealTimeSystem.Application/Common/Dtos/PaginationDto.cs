namespace RealTimeSystem.Application.Common.Dtos
{
    public record Pagination
    {
        private int _pageNumber;
        private int _pageSize;

        public int Page
        {
            get => PageNumber;
            set => PageNumber = value;
        }

        public int PageNumber
        {
            get => _pageNumber == 0 ? 1 : _pageNumber;
            set => _pageNumber = value;

        }

        public int PageSize
        {
            get => _pageSize == 0 ? 10 : _pageSize;
            set => _pageSize = value;
        }
    }
    public sealed record PaginationDto(int PageNumber, int PageSize, int TotalCount)
    {
        public int TotalPages => TotalCount <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, PaginationDto Pagination)
    {
        public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
            new(items, new PaginationDto(page, pageSize, totalCount));

        public int Page => Pagination.PageNumber;

        public int PageSize => Pagination.PageSize;

        public int TotalCount => Pagination.TotalCount;
    }
    public static class Paging
    {
        public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 100)
        {
            var normalizedPage = page < 1 ? 1 : page;
            var normalizedSize = pageSize < 1 ? 10 : Math.Min(pageSize, maxPageSize);
            return (normalizedPage, normalizedSize);
        }
    }
}
