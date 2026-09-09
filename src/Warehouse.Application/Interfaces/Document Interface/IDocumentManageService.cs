using System;
using System.Threading.Tasks;
using Warehouse.Application.DTO;
using Warehouse.Application.DTO.Paging;

namespace Warehouse.Application.Interfaces
{
    public interface IDocumentManageService
    {
        //CRUD
        Task<PagedResult<DocumentDTO>> GetAllAsync(PagingRequest request);
        Task<PagedResult<DocumentDTO>> GetByUserAsync(Guid userId, PagingRequest request);
        Task<DocumentDTO?> GetByIdAsync(Guid id);

        //create 
        Task<DocumentDTO> CreateReceiptAsync(CreateReceiptDTO dto, Guid createdBy);
        Task<DocumentDTO> CreateIssueAsync(CreateIssueDTO dto, Guid createdBy);
        Task<DocumentDTO> CreateTransferAsync(CreateTransferDTO dto, Guid createdBy);
        Task<DocumentDTO> CreateReversalAsync(CreateReversalDTO dto, Guid createdBy);
        Task<DocumentDTO> CreateOpeningAsync(CreateOpeningDTO dto, Guid createBy);
        Task<DocumentDTO> CreateAdjustmentAsync(CreateAdjustmentDTO dto, Guid createBy);
        
        //update 
        Task<bool> UpdateReceiptAsync(Guid id, UpdateReceiptDTO dto);
        Task<bool> UpdateIssueAsync(Guid id, UpdateIssueDTO dto);
        Task<bool> UpdateTransferAsync(Guid id, UpdateTransferDTO dto);
        Task<bool> UpdateOpeningAsync(Guid id, UpdateOpeningDTO dto);
        Task<bool> UpdateAdjustmentAsync(Guid id, UpdateAdjustmentDTO dto);

        //work flow
        Task<bool> SubmitAsync(Guid id);
        Task<bool> ApproveAsync(Guid id, Guid approverId, string approverRole);
        Task<bool> RejectAsync(Guid id, Guid rejectorId, string reason, string rejectorRole);
        Task<bool> CancelAsync(Guid id, Guid cancelerId, string cancelerRole);

    }
}