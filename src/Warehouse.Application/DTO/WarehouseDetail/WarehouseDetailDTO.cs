using Warehouse.Application.DTO.Paging;
namespace Warehouse.Application.DTO
{
    public class WarehouseDetailRequest : PagingRequest
    {
        public Guid WarehouseId { get; set; }
    }
    
    public class WarehouseDetailDTO
    {
        public Guid Id { get; set; } 
        public Guid MaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? UnitOfMeasureName { get; set; }
        public string? CategoryName { get; set; }
        public decimal Quantity { get; set; }
        public decimal MininumStock { get; set; } 
    }

}