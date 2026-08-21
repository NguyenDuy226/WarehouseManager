namespace Warehouse.Application.Interfaces
{
    public interface IStockPostingService
    {
        Task ApproveOpeningAsync(Guid documentId, Guid approvalId);
    }
}