using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentManageService _documentService;

        public DocumentController(IDocumentManageService documentService)
        {
            _documentService = documentService;
        }

        //CRUD
        [HttpGet]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, APPROVER")]
        public async Task<IActionResult> GetAll([FromQuery] PagingRequest request)
        {
            var result = await _documentService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetByUser([FromQuery] PagingRequest request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized("user not found");

            var result = await _documentService.GetByUserAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _documentService.GetByIdAsync(id);
            if (result == null) return NotFound("document not found");

            return Ok(result);
        }

        //create
        [HttpPost("receipt")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateReceipt([FromBody] CreateReceiptDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateReceiptAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("issue")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateIssue([FromBody] CreateIssueDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateIssueAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("transfer")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateTransferAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("adjustment")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateAdjustmentAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("opening")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateOpening([FromBody] CreateOpeningDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateOpeningAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("reversal")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> CreateReversal([FromBody] CreateReversalDTO dto)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _documentService.CreateReversalAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        //update
        [HttpPut("{id:guid}/receipt")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> UpdateReceipt(Guid id, [FromBody] UpdateReceiptDTO dto)
        {
            var success = await _documentService.UpdateReceiptAsync(id, dto);
            if (!success) return NotFound("document not found");
            return NoContent();
        }

        [HttpPut("{id:guid}/issue")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> UpdateIssue(Guid id, [FromBody] UpdateIssueDTO dto)
        {
            var success = await _documentService.UpdateIssueAsync(id, dto);
            if (!success) return NotFound("document not found");
            return NoContent();
        }

        [HttpPut("{id:guid}/transfer")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> UpdateTransfer(Guid id, [FromBody] UpdateTransferDTO dto)
        {
            var success = await _documentService.UpdateTransferAsync(id, dto);
            if (!success) return NotFound("document not found");
            return NoContent();
        }

        [HttpPut("{id:guid}/adjustment")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> UpdateAdjustment(Guid id, [FromBody] UpdateAdjustmentDTO dto)
        {
            var success = await _documentService.UpdateAdjustmentAsync(id, dto);
            if (!success) return NotFound("document not found");
            return NoContent();
        }

        [HttpPut("{id:guid}/opening")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> UpdateOpening(Guid id, [FromBody] UpdateOpeningDTO dto)
        {
            var success = await _documentService.UpdateOpeningAsync(id, dto);
            if (!success) return NotFound("document not found");
            return NoContent();
        }

        //work flow
        [HttpPost("{id:guid}/submit")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, REQUESTER")]
        public async Task<IActionResult> Submit(Guid id)
        {
            var success = await _documentService.SubmitAsync(id);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, APPROVER")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var approverId = GetUserId();
            if (approverId == Guid.Empty) return Unauthorized();

            var approverRole = GetUserRole();
            var success = await _documentService.ApproveAsync(id, approverId, approverRole);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, APPROVER")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDocumentRequest request)
        {
            var rejectorId = GetUserId();
            if (rejectorId == Guid.Empty) return Unauthorized();

            var rejectorRole = GetUserRole();
            var success = await _documentService.RejectAsync(id, rejectorId, request.Reason, rejectorRole);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var cancelerId = GetUserId();
            if (cancelerId == Guid.Empty) return Unauthorized();

            var cancelerRole = GetUserRole();
            var success = await _documentService.CancelAsync(id, cancelerId, cancelerRole);
            if (!success) return NotFound();

            return Ok();
        }

        //private method
        private Guid GetUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
        private string GetUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }

    public class RejectDocumentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
    
}