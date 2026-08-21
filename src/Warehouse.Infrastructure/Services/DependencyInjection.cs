using ExpenseTracker.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Services;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;
using Warehouse.Infrastructure.Services.Categories;
using Warehouse.Infrastructure.Services.Materials;
using Warehouse.Infrastructure.Services.Suppliers;
using Warehouse.Infrastructure.Services.UnitsOfMeasure;
using Warehouse.Infrastructure.Services.Warehouse;

namespace Warehouse.Infrastructure.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure (this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            services.AddDbContext<WarehouseDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<UserService>();
            services.AddScoped<AuthService>();
            services.AddScoped<CodeGenerator>();

            services.AddScoped<IMaterialService, MaterialService>();
            services.AddScoped<IMaterialCategoryService, MaterialCategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IBaseDocumentService, BaseDocumentService>();
            services.AddScoped<IOpeningDocumentService, OpeningDocumentService>();
            return services;
        }
    }
}