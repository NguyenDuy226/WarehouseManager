using System;
using System.Threading.Tasks;
using Warehouse.Application.DTO;

namespace Warehouse.Application.Interfaces
{
    public interface IOpeningDocumentService
    {
        Task<Guid> CreateOpeningAsync(CreateOpeningRequest request, string userId);
        Task ApproveOpeningAsync(Guid documentId, Guid approverId, string approverRole);
    }
}