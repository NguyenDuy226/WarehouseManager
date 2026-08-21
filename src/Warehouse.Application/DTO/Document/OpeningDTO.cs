using System;
using System.Collections.Generic;

namespace Warehouse.Application.DTO
{
    public class CreateOpeningRequest
    {
        public Guid WarehouseId { get; set; }
        public List<OpeningLineRequest> Lines { get; set; } = new();
    }

    public class OpeningLineRequest
    {
        public Guid MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}