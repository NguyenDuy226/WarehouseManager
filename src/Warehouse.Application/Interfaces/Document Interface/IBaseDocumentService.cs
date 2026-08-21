using Warehouse.Domain.Entities;

namespace Warehouse.Application.Interfaces
{
    public interface IBaseDocumentService
    {
        Task PostDocumentAsync(StockDocument document);
    }
}