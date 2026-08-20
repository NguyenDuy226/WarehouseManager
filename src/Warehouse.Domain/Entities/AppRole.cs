using Microsoft.AspNetCore.Identity;

namespace Warehouse.Domain.Entities;
public class AppRole : IdentityRole<Guid>
{
    public ICollection<AppUser> Users { get; set; } = [];
}