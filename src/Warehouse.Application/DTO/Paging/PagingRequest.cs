namespace Warehouse.Application.DTO.Paging
{
    public class PagingRequest
    {
        const int maxPageSize = 50;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize { 
            get => _pageSize; 
            set => _pageSize = (value > maxPageSize) ? maxPageSize : (value < 1 ? 1 : value);
        }
        public string? Keyword { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public string? SortBy { get; set; } = "createdAt"; 
        public string? SortDirection { get; set; } = "desc";
    }
}