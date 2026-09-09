using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces
{
    public interface IDocumentProcessService
    {
        Task PostDocumentAsync(StockDocument document);
    }
}