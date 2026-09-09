using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application.DTO;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services.Document
{
    public class DocumentManageService : IDocumentManageService
    {
        private readonly WarehouseDbContext _context;
        private readonly CodeGenerator _codeGenerator;
        private readonly IDocumentProcessService _processService;
        private readonly UserManager<AppUser> _userManager;

        public DocumentManageService(WarehouseDbContext context, CodeGenerator codeGenerator, IDocumentProcessService processService, UserManager<AppUser> userManager)
        {
            _context = context;
            _codeGenerator = codeGenerator;
            _processService = processService;
            _userManager = userManager;
        }

        //CRUD
        public async Task<PagedResult<DocumentDTO>> GetAllAsync(PagingRequest request)
        {
            var query = _context.Set<StockDocument>().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var sortDirection = request.SortDirection?.ToLower() == "asc" ? "asc" : "desc";
            var sortBy = request.SortBy?.ToLower() ?? "createat";

            query = sortBy switch
            {
                "code" => sortDirection == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "type" => sortDirection == "asc" ? query.OrderBy(x => x.Type) : query.OrderByDescending(x => x.Type),
                "status" => sortDirection == "asc" ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => sortDirection == "asc" ? query.OrderBy(x => x.DocumentDate) : query.OrderByDescending(x => x.DocumentDate)
            };

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var items = await query
                .Where(t => t.Status != 0)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new DocumentDTO
                {
                    Id = t.Id,
                    Code = t.Code,
                    Type = t.Type,
                    Status = t.Status,
                    WarehouseId = t.WarehouseId,
                    CreatedAt = t.DocumentDate
                })
                .ToListAsync();

            return new PagedResult<DocumentDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<PagedResult<DocumentDTO>> GetByUserAsync(Guid userId, PagingRequest request)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId cannot be empty");

            var userIdStr = userId.ToString();
            var query = _context.Set<StockDocument>()
                .AsNoTracking()
                .Where(x => x.CreatedBy == userIdStr); 

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var sortDirection = request.SortDirection?.ToLower() == "asc" ? "asc" : "desc";
            var sortBy = request.SortBy?.ToLower() ?? "createat";

            query = sortBy switch
            {
                "code" => sortDirection == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                "type" => sortDirection == "asc" ? query.OrderBy(x => x.Type) : query.OrderByDescending(x => x.Type),
                "status" => sortDirection == "asc" ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => sortDirection == "asc" ? query.OrderBy(x => x.DocumentDate) : query.OrderByDescending(x => x.DocumentDate)
            };

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new DocumentDTO
                {
                    Id = t.Id,
                    Code = t.Code,
                    Type = t.Type,
                    Status = t.Status,
                    WarehouseId = t.WarehouseId,
                    CreatedAt = t.DocumentDate
                })
                .ToListAsync();

            return new PagedResult<DocumentDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<DocumentDTO?> GetByIdAsync(Guid id)
        {
            var item = await _context.Set<StockDocument>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include("Supplier")
                .Include(x => x.Lines)
                    .ThenInclude(l => l.Material)
                        .ThenInclude(u => u!.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return null; 
            }
            var creatorName = await _userManager.FindByIdAsync(item.CreatedBy);
            string displayName = creatorName != null ? creatorName.Name : "Nhân viên đã bị khóa";
            if(item.Warehouse == null)
            {
                throw new ArgumentException("warehouse not found");
            }
            var dto = new DocumentDTO
            {
                Id = item.Id,
                Code = item.Code,
                Type = item.Type,
                Status = item.Status,
                WarehouseId = item.WarehouseId,
                WarehouseName = item.Warehouse.Name,
                CreatedAt = item.DocumentDate,
                CreatedBy = item.CreatedBy,
                CreatorName = displayName,
                Reason = item.Reason ?? string.Empty,
                Note = item.Note ?? string.Empty,

                //receipt
                SupplierId = item is ReceiptDocument ? ((ReceiptDocument)item).SupplierId : null,
                SupplierName = item is ReceiptDocument receipt && receipt.Supplier != null ? receipt.Supplier.Name : string.Empty, 
                ExternalDocumentNo = item is ReceiptDocument ? ((ReceiptDocument)item).ExternalDocumentNo : (item is IssueDocument ? ((IssueDocument)item).ExternalDocumentNo : null),
                //issue
                Receiver = item is IssueDocument ? ((IssueDocument)item).Receiver : null,
                //transfer
                ToWarehouseId = item is TransferDocument ? ((TransferDocument)item).ToWarehouseId : null,
                
                Lines = item.Lines
                        .Where(l => !l.IsRemoved)
                        .Select(l => new DocumentLineDTO
                        {
                            Id = l.Id,
                            MaterialId = l.MaterialId,
                            Quantity = l.Quantity,
                            UnitPrice = l.UnitPrice,
                            ItemCode = l.Material != null ? l.Material.Code : string.Empty,
                            ItemName = l.Material != null ? l.Material.Name : string.Empty,
                            Unit = (l.Material != null && l.Material.UnitOfMeasure != null) ? l.Material.UnitOfMeasure.Name : string.Empty           
                        })
                        .ToList()

            };

            return dto;
        }
        
        //create
        public async Task<DocumentDTO> CreateReceiptAsync(CreateReceiptDTO dto, Guid createdBy)
        {
            var document = new ReceiptDocument(createdBy.ToString(), dto.WarehouseId);
            var warehouse = await _context.WarehouseEntities.AsNoTracking()
                                                            .FirstOrDefaultAsync(t => t.Id == dto.WarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            var documentCode = await _codeGenerator.GenerateDocumentCode(DocumentType.RECEIPT, warehouse.Code);
            document.SetCode(documentCode);
            document.SetReceiptDetails(dto.SupplierId, dto.ExternalDocumentNo, dto.DocumentDate, dto.Reason, dto.Note);
            
            LineAction(document, dto.Lines);

            await _context.Set<StockDocument>().AddAsync(document);
            await _context.SaveChangesAsync();

            return Response(document);
        }
        public async Task<DocumentDTO> CreateIssueAsync(CreateIssueDTO dto, Guid createdBy)
        {
            var document = new IssueDocument(createdBy.ToString(), dto.WarehouseId);    
            
            var warehouse = await _context.WarehouseEntities.AsNoTracking()
                                                            .FirstOrDefaultAsync(t => t.Id == dto.WarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            var documentCode = await _codeGenerator.GenerateDocumentCode(DocumentType.ISSUE, warehouse.Code);
            document.SetCode(documentCode);

            document.SetIssueDetails(dto.Receiver, dto.ExternalDocumentNo, dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.Set<StockDocument>().AddAsync(document);
            await _context.SaveChangesAsync();

            return Response(document);
        }
        public async Task<DocumentDTO> CreateTransferAsync(CreateTransferDTO dto, Guid createdBy)
        {
            var warehouse = await _context.WarehouseEntities.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.WarehouseId);
            var toWarehouse = await _context.WarehouseEntities.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.ToWarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            if(toWarehouse == null) throw new ArgumentException("toWarehouse not found");
            if(warehouse.Status == EntityStatus.Inactive || toWarehouse.Status == EntityStatus.Inactive)
            {
                throw new ArgumentException("warehouse is locked");
            }

            var document = new TransferDocument(createdBy.ToString(), dto.WarehouseId, dto.ToWarehouseId);
            var documentCode = await _codeGenerator.GenerateDocumentCode(DocumentType.TRANSFER, warehouse.Code);
            document.SetCode(documentCode);          
            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.Set<StockDocument>().AddAsync(document);
            await _context.SaveChangesAsync();

            return Response(document);
        }
        public async Task<DocumentDTO> CreateReversalAsync(CreateReversalDTO dto, Guid createdBy)
        {
            var originalDoc = await _context.Set<StockDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == dto.OriginalDocumentId);

            if (originalDoc == null) throw new ArgumentException("original document not found");

            var warehouse = await _context.WarehouseEntities.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == originalDoc.WarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            var reversalCode = await _codeGenerator.GenerateDocumentCode(DocumentType.REVERSAL, warehouse.Code);
            var reversalDoc = originalDoc.CreateReversalDocument(createdBy.ToString(), reversalCode);
            reversalDoc.SetCode(reversalCode);           
            reversalDoc.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);

            await _context.Set<StockDocument>().AddAsync(reversalDoc);
            await _context.SaveChangesAsync();

            return Response(reversalDoc);
        }        
        public async Task<DocumentDTO> CreateAdjustmentAsync(CreateAdjustmentDTO dto, Guid createdBy)
        {
            var document = new AdjustmentDocument(createdBy.ToString(), dto.WarehouseId);
            var warehouse = await _context.WarehouseEntities.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.WarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            var documentCode = await _codeGenerator.GenerateDocumentCode(DocumentType.ADJUSTMENT, warehouse.Code);
            document.SetCode(documentCode);
            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.Set<StockDocument>().AddAsync(document);
            await _context.SaveChangesAsync();

            return Response(document);
        }
        public async Task<DocumentDTO> CreateOpeningAsync(CreateOpeningDTO dto, Guid createdBy)
        {
            var document = new OpeningDocument(createdBy.ToString(), dto.WarehouseId);
            
            var warehouse = await _context.WarehouseEntities.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.WarehouseId);
            if(warehouse == null) throw new ArgumentException("warehouse not found");
            var documentCode = await _codeGenerator.GenerateDocumentCode(DocumentType.OPENING, warehouse.Code);
            
            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.Set<StockDocument>().AddAsync(document);
            await _context.SaveChangesAsync();

            return Response(document);
        }

        //update 
        public async Task<bool> UpdateReceiptAsync(Guid id, UpdateReceiptDTO dto)
        {
            var document = await _context.Set<ReceiptDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);
           
            if (document == null) return false;
            document.SetReceiptDetails(dto.SupplierId, dto.ExternalDocumentNo, dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);
            
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateIssueAsync(Guid id, UpdateIssueDTO dto)
        {
            var document = await _context.Set<IssueDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null) return false;
            document.SetIssueDetails(dto.Receiver, dto.ExternalDocumentNo, dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateTransferAsync(Guid id, UpdateTransferDTO dto)
        {
            var document = await _context.Set<TransferDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null) return false;
            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateAdjustmentAsync(Guid id, UpdateAdjustmentDTO dto)
        {
            var document = await _context.Set<AdjustmentDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null) return false;

            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateOpeningAsync(Guid id, UpdateOpeningDTO dto)
        {
            var document = await _context.Set<OpeningDocument>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (document == null) return false;

            document.SetDocumentDetails(dto.DocumentDate, dto.Reason, dto.Note);
            LineAction(document, dto.Lines);

            await _context.SaveChangesAsync();
            return true;
        }
        
        //privae method
        private void LineAction(StockDocument document, List<DocumentLineCreateDTO> linesDto)
        {
            linesDto ??= new List<DocumentLineCreateDTO>();
            var incomingIds = linesDto
                .Select(l => l.Id!.Value)
                .ToList();

            var linesToRemove = document.Lines
                .Where(l => !l.IsRemoved && !incomingIds.Contains(l.Id))
                .ToList(); 
            foreach (var line in linesToRemove)
            {
                line.MarkAsRemoved(); 
            }

            foreach (var dto in linesDto)
            {
                if (dto.Id == null || dto.Id == Guid.Empty)
                {
                    document.AddOrUpdateLine(dto.MaterialId, dto.Quantity, dto.UnitPrice);
                }
                else
                {
                    var existingLine = document.Lines.FirstOrDefault(l => l.Id == dto.Id);
                    if (existingLine != null)
                    {
                        if (existingLine != null)
                        {
                            existingLine.Update(dto.MaterialId, dto.Quantity, dto.UnitPrice);
                        }
                    }
                }
            }
        }
        private DocumentDTO Response(StockDocument item)
        {
            return new DocumentDTO
            {
                Id = item.Id,
                Code = item.Code,
                Type = item.Type,
                Status = item.Status,
                WarehouseId = item.WarehouseId,
                CreatedAt = item.DocumentDate,
                Reason = item.Reason,
                Note = item.Note,
                Lines = item.Lines?.Where(l => !l.IsRemoved).Select(l => new DocumentLineDTO
                {
                    Id = l.Id,
                    MaterialId = l.MaterialId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice
                }).ToList() ?? new List<DocumentLineDTO>()
            };
        }

        //work flow
        public async Task<bool> SubmitAsync(Guid id)
        {
            var document = await _context.Set<StockDocument>()
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (document == null) return false;

            document.SubmitForApproval();
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ApproveAsync(Guid id, Guid approverId, string approverRole)
        {
            var document = await _context.Set<StockDocument>()
                .Include(d => d.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (document == null) return false;

            document.Approve(approverId.ToString(), approverRole);

            await _context.Set<ApprovalHistory>().AddAsync(new ApprovalHistory(document.Id, approverId, "APPROVE", "Duyệt chứng từ"));
            await _context.SaveChangesAsync();

            await _processService.PostDocumentAsync(document);
            return true;
        }
        public async Task<bool> RejectAsync(Guid id, Guid rejectorId, string reason, string rejectorRole)
        {
            var document = await _context.Set<StockDocument>().FindAsync(id);
            if (document == null) return false;
            document.Reject(rejectorId.ToString(), reason, rejectorRole);
            await _context.Set<ApprovalHistory>().AddAsync(new ApprovalHistory(document.Id, rejectorId, "REJECT", reason));
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CancelAsync(Guid id, Guid cancelerId, string cancelerRole)
        {
            var document = await _context.Set<StockDocument>().FindAsync(id);
            if (document == null) return false;

            document.Cancel(cancelerId.ToString(), cancelerRole);
            await _context.Set<ApprovalHistory>().AddAsync(new ApprovalHistory(document.Id, cancelerId, "CANCEL", "Xóa chứng từ"));
            await _context.SaveChangesAsync();
            return true;
        }


    }
}