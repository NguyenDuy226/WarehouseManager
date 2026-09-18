using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;
using Warehouse.Application.DTO.User;
using Warehouse.Application.DTO.Paging;
using System.Security.Claims;
using Warehouse.Application.DTO;

namespace Warehouse.Application.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly WarehouseDbContext _context;

        public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, WarehouseDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        private async Task<bool> AuthCheck(AppUser targetUser, ClaimsPrincipal currentUser)
        {
            var isAdmin = currentUser.IsInRole("SYSTEM_ADMIN");
            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var isHigherLevel = targetRoles.Contains("WAREHOUSE_MANAGER") || targetRoles.Contains("SYSTEM_ADMIN");
            if (!isAdmin && isHigherLevel)
            {
                return false;
            }
            return true;
        }

        public async Task<(int StatusCode, object Response)> SetRoleToUserAsync(Guid userId, RoleRequestDTO roleDTO, ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (404, new { message = "user not found" });

            if (!await AuthCheck(user, currentUser))
            {
                return (403, new { message = "only admin can set role to manager" });
            }

            var role = await _roleManager.FindByNameAsync(roleDTO.Role);
            if (role == null) return (404, new { message = "role not found" });

            var currRole = await _userManager.GetRolesAsync(user);
            if (currRole.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currRole);
            }

            var result = await _userManager.AddToRoleAsync(user, roleDTO.Role);
            if (!result.Succeeded) return (500, new { message = "set role error" });
            await _userManager.UpdateSecurityStampAsync(user);
            return (200, new { message = "role updated success" });
        }

        public async Task<(int StatusCode, object Response)> SetWarehouseToUserAsync(Guid userId, List<WarehousesToUserRequestDTO> dto, ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (404, new { message = "user not found" });
            if (!await AuthCheck(user, currentUser))
            {
                return (403, new { message = "only admin can do this" });
            }
            var allowedRoles = new List<string> { "SYSTEM_ADMIN", "WAREHOUSE_MANAGER", "WAREHOUSE_CLERK", "APPROVER", "REQUESTER", "AUDITOR" };
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Intersect(allowedRoles).Any())
            {
                return (400, new { message = "invalid role for warehouse" });
            }
            var warehouses = dto.Select(d => Guid.Parse(d.WarehouseId)).ToList();

            var existPer = await _context.WarehousePermissions
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var item in existPer)
            {
                if (warehouses.Contains(item.WarehouseId))
                {
                    item.IsRemoved = false;
                }
                else
                {
                    item.IsRemoved = true;
                }
            }
            var existWarehouseId = existPer.Select(p => p.WarehouseId).ToList();
            var newWarehouseId = warehouses.Except(existWarehouseId).ToList();
            if (newWarehouseId.Any())
            {
                var newPer = newWarehouseId.Select(id => new WarehousePermission
                {
                    UserId = userId,
                    WarehouseId = id,
                    IsRemoved = false
                });
                await _context.WarehousePermissions.AddRangeAsync(newPer);
            }
            await _context.SaveChangesAsync();
            
            return (200, new { message = "warehouse assign successful" });
        }

        public async Task<(int StatusCode, object Response)> SetUsersToWarehouseAsync(Guid warehouseId, List<UsersToWarehouseDTO> dto, ClaimsPrincipal currentUser)
        {
            var warehouse = await _context.WarehouseEntities.FindAsync(warehouseId);
            if (warehouse == null) 
            {
                return (404, new { message = "Warehouse not found" });
            }
            var userIds = dto.Select(d => d.UserId).ToList();
            var allowedRoles = new List<string> { "SYSTEM_ADMIN", "WAREHOUSE_MANAGER", "WAREHOUSE_CLERK", "APPROVER", "REQUESTER", "AUDITOR", "USER" };
            if (userIds.Any())
            {
                var usersToCheck = await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new 
                    {
                        u.Id,
                        u.IsActive
                    })
                    .ToListAsync();
                // if (usersToCheck.Any(u => u.IsActive == false)) 
                // {
                //     return (400, new { message = "user is not active" });
                // }
                // var invalidUsersCount = await _context.Users
                //     .Where(u => userIds.Contains(u.Id))
                //     .Where(u => _context.UserRoles
                //                 .Where(ur => ur.UserId == u.Id)
                //                 .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                //                 .Any(role => (role ?? "").ToUpper() == "USER")
                //     )
                //     .CountAsync();
                // if (invalidUsersCount > 0)
                // {
                //     return (400, new { message = "user invalid" });
                // }
            }
            var warehouseIdStr = warehouseId.ToString();
            var existPermissions = await _context.WarehousePermissions
                .Where(t => t.WarehouseId.ToString() == warehouseIdStr)
                .ToListAsync();
            foreach (var item in existPermissions)
            {
                item.IsRemoved = !userIds.Contains(item.UserId);
            }
            var existUserIds = existPermissions.Select(p => p.UserId).ToHashSet(); 
            var newUserIds = userIds.Where(id => !existUserIds.Contains(id)).ToList();
            if (newUserIds.Any())
            {
                var newPermissions = newUserIds.Select(id => new WarehousePermission
                {
                    WarehouseId = warehouseId,
                    UserId = id,
                    IsRemoved = false
                });

                await _context.WarehousePermissions.AddRangeAsync(newPermissions);
            }
            await _context.SaveChangesAsync();
            return (200, "");
        }

        public async Task<(int StatusCode, object Response)> UpdateUserAsync(Guid userId, UpdateUserRequestDTO dto, ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (404, new { message = "user not found" });
            if (!await AuthCheck(user, currentUser))
            {
                return (403, new { message = "only admin can do this" });
            }
            user.Name = dto.Name;
            user.IsActive = dto.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (500, new { message = "update user error" });
            await _userManager.UpdateSecurityStampAsync(user);
            return (200, new { message = "user updated" });
        }

        public async Task<(int StatusCode, object Response)> DeleteUserAsync(Guid userId, ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return (404, new { message = "user not found" });
            if (!await AuthCheck(user, currentUser))
            {
                return (403, new { message = "only admin can do this" });
            }
            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (500, new { message = "delete user error" });
            await _userManager.UpdateSecurityStampAsync(user);
            return (200, new { message = "user delete successful" });
        }

        public async Task<PagedResult<UserDTO>> GetAllAsync(PagingRequest request)
        {
            var query = _userManager.Users.AsQueryable();
            //remove admin
            var adminRole = await _roleManager.FindByNameAsync("SYSTEM_ADMIN");
            if (adminRole != null)
            {
                var adminUserIds = _context.UserRoles
                    .Where(t => t.RoleId == adminRole.Id)
                    .Select(t => t.UserId);
                query = query.Where(t => !adminUserIds.Contains(t.Id));
            }
            //search
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower(); 
                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(keyword)) ||
                    (u.Code != null && u.Code.ToLower().Contains(keyword))
                );
            }
            if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "all")
            {
                bool isActive = request.Status == "active";
                query = query.Where(u => u.IsActive == isActive);
            }
            if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != "all")
            {
                var role = await _roleManager.FindByNameAsync(request.Role);
                if (role != null)
                {
                    var userIdsInRole = _context.UserRoles
                        .Where(ur => ur.RoleId == role.Id)
                        .Select(ur => ur.UserId);
                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
            }
            if (!string.IsNullOrWhiteSpace(request.SortDirection) && request.SortDirection.ToLower() == "asc")
            {
                query = query.OrderBy(u => u.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }
            //paging
            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            var userRolesMapping = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where userIds.Contains(ur.UserId)
                select new { ur.UserId, RoleName = r.Name }
            ).ToListAsync();
            var userRolesDict = userRolesMapping
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).ToList());

            var items = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Code = u.Code ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Name = u.Name ?? string.Empty,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = userRolesDict.ContainsKey(u.Id) ? userRolesDict[u.Id] : new List<string>()
            }).ToList();

            return new PagedResult<UserDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PagedResult<UserDTO>> GetValidUserAsync(PagingRequest request)
        {
            //not user and admin
            var allowedRoles = new[] {"WAREHOUSE_MANAGER", "WAREHOUSE_CLERK", "APPROVER", "REQUESTER", "AUDITOR", "USER"};
            var query = _userManager.Users.Where(u =>_context.UserRoles.Any(ur => ur.UserId == u.Id 
                                                    && _context.Roles.Any(r =>r.Id == ur.RoleId && allowedRoles.Contains(r.Name!))));
            //search, sort
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(keyword)) ||
                    (u.Code != null && u.Code.ToLower().Contains(keyword))
                );
            }
            query = request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt);

            //paging
            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();
            var userRolesMapping = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where userIds.Contains(ur.UserId)
                select new
                {
                    ur.UserId,
                    RoleName = r.Name,
                
                }
            ).ToListAsync();
            var userRolesDict = userRolesMapping
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.RoleName).ToList()
                );

            var items = users.Select(u => new UserDTO
            {
                Id = u.Id,
                Code = u.Code ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Name = u.Name ?? string.Empty,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = userRolesDict.GetValueOrDefault(u.Id) ?? new List<string>()
            }).ToList();

            return new PagedResult<UserDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
            
        public async Task<UserDTO?> GetByIdAsync(Guid id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            var warehouses = await _context.WarehousePermissions
                .Include(wp => wp.Warehouse)
                .Where(wp => wp.UserId == id && wp.IsRemoved == false && wp.Warehouse != null)
                .Select(wp => new UserWarehouseDTO
                {
                    Id = wp.WarehouseId,
                    Code = wp.Warehouse!.Code ?? string.Empty,
                    Name = wp.Warehouse!.Name ?? string.Empty,
                    Status = wp.Warehouse!.Status
                })
                .ToListAsync();
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name ?? string.Empty,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = roles,
                Warehouses = warehouses
            };
        }
    
        public async Task<List<string>> GetUserWarehousesAsync(Guid userId)
        {
            return await _context.WarehousePermissions
                .Where(wp => wp.UserId == userId && wp.IsRemoved == false)
                .Select(wp => wp.WarehouseId.ToString())
                .ToListAsync();
        }
        
        public async Task<List<string>> GetWarehouUsersAsync(Guid warehouseId)
        {
            return await _context.WarehousePermissions
                .Where(wp => wp.WarehouseId == warehouseId && !wp.IsRemoved)
                .Select(wp => wp.UserId.ToString()) 
                .ToListAsync();
        }

    }
}