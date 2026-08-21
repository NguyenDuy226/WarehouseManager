using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.DTO.User;
using Warehouse.Application.DTO.Paging;
using Warehouse.Application.Services;

namespace Warehouse.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "SYSTEM_ADMIN, WAREHOUSE_MANAGER")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagingRequest request)
        {
            var result = await _userService.GetAllAsync(request);
            return Ok(result);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var result = await _userService.GetByIdAsync(userId);
            if (result == null) return NotFound(new { message = "user not found" });
            return Ok(result);
        }

        [HttpPut("{userId:guid}/role")]
        public async Task<IActionResult> SetRoleToUser(Guid userId, [FromBody] RoleRequestDTO roleDTO)
        {
            var (statusCode, response) = await _userService.SetRoleToUserAsync(userId, roleDTO, User);
            return StatusCode(statusCode, response);
        }

        [HttpPost("{userId:guid}/warehouse")]
        public async Task<IActionResult> SetWarehouseToUser(Guid userId, [FromBody] List<WarehousesToUserRequestDTO> dto)
        {
            var (statusCode, response) = await _userService.SetWarehouseToUserAsync(userId, dto, User);
            return StatusCode(statusCode, response);
        }

        [HttpPost("/api/warehouses/{warehouseId:guid}/users")]
        public async Task<IActionResult> SetUsersToWarehouse(Guid warehouseId, [FromBody] List<UsersToWarehouseDTO> dto)
        {
            var (statusCode, response) = await _userService.SetUsersToWarehouseAsync(warehouseId, dto, User);
            return StatusCode(statusCode, response);
        }

        [HttpPut("{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequestDTO dto)
        {
            var (statusCode, response) = await _userService.UpdateUserAsync(userId, dto, User);
            return StatusCode(statusCode, response);
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var (statusCode, response) =await _userService.DeleteUserAsync(userId, User);
            return StatusCode(statusCode, response);
        }

        [HttpGet("{userId:guid}/warehouses")]
        public async Task<IActionResult> GetUserWarehouses(Guid userId)
        {
            var result = await _userService.GetUserWarehousesAsync(userId);
            return Ok(result);
        }
        [HttpGet("valid-users")]
        public async Task<IActionResult> GetValidUsers([FromQuery] PagingRequest request)
        {
            var result = await _userService.GetValidUserAsync(request);
            return Ok(result);
        }
        [HttpGet("{id:guid}/users")]
        public async Task<IActionResult> GetWarehouseUsers(Guid id)
        {
            var userIds = await _userService.GetWarehouUsersAsync(id);
            return Ok(userIds); 
        }


    }
}