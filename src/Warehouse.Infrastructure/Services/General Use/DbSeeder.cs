using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Inventory.Enums;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services;

public class DbSeeder
{
    private readonly WarehouseDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger<DbSeeder> _logger;
    private readonly CodeGenerator _codeGenerator;

    public DbSeeder(WarehouseDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<DbSeeder> logger, CodeGenerator codeGenerator) 
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _codeGenerator = codeGenerator;
    }

    public async Task Seeding()
    {
        try
        {
            await RoleSeeding();
            await AdminSeeding();
            await SeedingUser();
            
            await SeedingUnitOfMeasures();
            await SeedingMaterialCategories();
            await SeedingSuppliers();
            await SeedingMaterials(); 
            await SeedingWarehouses();
        }
        catch (Exception ex)
        {   
            _logger.LogError(ex, "error seeding DB");
            throw;
        }
    }

    public async Task RoleSeeding()
    {
        string[] roles = ["SYSTEM_ADMIN", "WAREHOUSE_MANAGER", "WAREHOUSE_CLERK", "APPROVER", "REQUESTER", "AUDITOR", "USER"];
        foreach (var item in roles)
        {
            if (!await _roleManager.RoleExistsAsync(item))
            {
                var role = new AppRole
                {
                    Name = item,
                    NormalizedName = item.ToUpper(),
                };
                await _roleManager.CreateAsync(role);
            }            
        }
    }

    public async Task AdminSeeding()
    {
        var adminEmail = "admin@gmail.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                Name = "ADMIN",
                IsActive = true,
                UserName = adminEmail,
                Code = await _codeGenerator.GenerateCode<AppUser>() 
            };
            
            var result = await _userManager.CreateAsync(adminUser, "Aa123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "SYSTEM_ADMIN");
            }
        }
    }

    public async Task SeedingUser()    
    {
        if (await _userManager.Users.CountAsync() > 1) return;

        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.IsActive, f => true)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        var fakeUsers = userFaker.Generate(100);

        foreach (var user in fakeUsers)
        {
            user.Code = await _codeGenerator.GenerateCode<AppUser>();
            var result = await _userManager.CreateAsync(user, "Aa123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "USER");
            }
        }
    }

    public async Task SeedingMaterialCategories()
    {
        if (await _context.MaterialCategories.AnyAsync()) return;

        var faker = new Faker<MaterialCategory>()
            .RuleFor(c => c.Id, f => Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0] + " " + f.Random.Word())
            .RuleFor(c => c.Description, f => f.Commerce.ProductDescription())
            .RuleFor(c => c.Status, f => EntityStatus.Active)
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        var fakes = faker.Generate(20);
        foreach (var item in fakes)
        {
            item.Code = await _codeGenerator.GenerateCode<MaterialCategory>();
        }

        await _context.MaterialCategories.AddRangeAsync(fakes);
        await _context.SaveChangesAsync();
    }

    public async Task SeedingUnitOfMeasures()
    {
        if (await _context.UnitOfMeasures.AnyAsync()) return;

        var predefinedUnits = new List<string> { "Hộp", "Cái", "Mét", "Lít", "Kg", "Gói", "Cuộn", "Bộ", "Chai", "Lon", "Tuýp", "Thùng" };
        var units = new List<UnitOfMeasure>();
        var dateFaker = new Faker();

        foreach (var unitName in predefinedUnits)
        {
            units.Add(new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Name = unitName,
                Code = await _codeGenerator.GenerateCode<UnitOfMeasure>(),
                Status = EntityStatus.Active,
                CreatedAt = dateFaker.Date.Past(2).ToUniversalTime()
            });
        }

        await _context.UnitOfMeasures.AddRangeAsync(units);
        await _context.SaveChangesAsync();
    }

    public async Task SeedingSuppliers()
    {
        if (await _context.Suppliers.AnyAsync()) return;

        var faker = new Faker<Supplier>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.Name, f => f.Company.CompanyName())
            .RuleFor(s => s.TaxCode, f => f.Random.Replace("##########")) 
            .RuleFor(s => s.Addres, f => f.Address.FullAddress())
            .RuleFor(s => s.Contact, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.Status, f => EntityStatus.Active)
            .RuleFor(s => s.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        var fakes = faker.Generate(10);
        foreach (var item in fakes)
        {
            item.Code = await _codeGenerator.GenerateCode<Supplier>();
        }

        await _context.Suppliers.AddRangeAsync(fakes);
        await _context.SaveChangesAsync();
    }

    public async Task SeedingMaterials()
    {
        if (await _context.Materials.AnyAsync()) return;
        var categoryIds = await _context.MaterialCategories.Select(c => c.Id).ToListAsync();
        var unitIds = await _context.UnitOfMeasures.Select(u => u.Id).ToListAsync();

        if (!categoryIds.Any() || !unitIds.Any()) return; 

        var faker = new Faker<Material>()
            .RuleFor(m => m.Id, f => Guid.NewGuid())
            .RuleFor(m => m.Name, f => f.Commerce.ProductName())
            .RuleFor(m => m.CategoryId, f => f.PickRandom(categoryIds)) 
            .RuleFor(m => m.UnitOfMeasureId, f => f.PickRandom(unitIds)) 
            .RuleFor(m => m.RefPrice, f => f.Finance.Amount(10000, 1000000))
            .RuleFor(m => m.MininumStock, f => f.Random.Decimal(1, 100)) 
            .RuleFor(m => m.Status, f => EntityStatus.Active)
            .RuleFor(m => m.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        var fakes = faker.Generate(20); 
        foreach (var item in fakes)
        {
            item.Code = await _codeGenerator.GenerateCode<Material>();
        }

        await _context.Materials.AddRangeAsync(fakes);
        await _context.SaveChangesAsync();
    }

    public async Task SeedingWarehouses()
    {
        if (await _context.WarehouseEntities.AnyAsync()) return;

        var faker = new Faker<WarehouseEntity>()
            .RuleFor(w => w.Id, f => Guid.NewGuid())
            .RuleFor(w => w.Name, f => "Warehouse " + f.Company.CompanyName())
            .RuleFor(w => w.Address, f => f.Address.FullAddress())
            .RuleFor(w => w.Manager, f => f.Name.FullName())
            .RuleFor(w => w.CreatedAt, f => f.Date.Past(2).ToUniversalTime());

        var fakes = faker.Generate(100);
        foreach (var item in fakes)
        {
            item.Code = await _codeGenerator.GenerateCode<WarehouseEntity>();
        }
        await _context.WarehouseEntities.AddRangeAsync(fakes);
        await _context.SaveChangesAsync();
    }

}