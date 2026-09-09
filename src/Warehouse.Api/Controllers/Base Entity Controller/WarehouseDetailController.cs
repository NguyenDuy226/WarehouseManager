using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO;
using Warehouse.Application.Interfaces;

namespace Warehouse.Api.Controllers
{
    [Authorize (Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER, WAREHOUSE_CLERK, APPROVER, REQUESTER, AUDITOR")]
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseDetailController : ControllerBase
        {
        private readonly IWarehouseDetailService _detailService;

        public WarehouseDetailController(IWarehouseDetailService detailService)
        {
            _detailService = detailService;
        }
        [HttpGet]
        public async Task<IActionResult> GetDetailAsync([FromQuery] WarehouseDetailRequest request)
        {
            if (request.WarehouseId == Guid.Empty)
            {
                return BadRequest();
            }

            var result = await _detailService.GetDetailAsync(request);
            return Ok(result);
        }
    }
}