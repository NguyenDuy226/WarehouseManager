using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure.Services
{
    public class CodeGenerator
    {
        private readonly WarehouseDbContext _context;
        private readonly Dictionary<Type, (string SequenceName, string Prefix)> _config = new()
        {
            { typeof(WarehouseEntity), ("WarehouseCodeSeq", "WH-") },
            { typeof(Material), ("MaterialCodeSeq", "MT") },
            { typeof(MaterialCategory), ("MaterialCategoryCodeSeq", "MC-") },
            { typeof(Supplier), ("SupplierCodeSeq", "SP-") },
            { typeof(UnitOfMeasure), ("UnitOfMeasureCodeSeq", "UN-") },
            { typeof(AppUser), ("UserCodeSeq", "US-") },
        };

        public CodeGenerator(WarehouseDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateCode<T>(int numberLength = 5) where T : class
        {
            var entityType = typeof(T);
            if (!_config.TryGetValue(entityType, out var setting))
            {
                throw new Exception($"chua cau hinh sequence {entityType.Name}");
            }
            string sequenceName = setting.SequenceName;
            string prefix = setting.Prefix;
            var connection = _context.Database.GetDbConnection();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT nextval('\"{sequenceName}\"')";            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            var nextValue = (long)(await command.ExecuteScalarAsync())!;

            return $"{prefix}{nextValue.ToString().PadLeft(numberLength, '0')}";
        }
    
        public async Task<string> GenerateDocumentCode(DocumentType type, string warehouseCode, int numberLength = 6)
        {
            string sequenceName = "StockDocumentSeq";
            string currentYear = DateTime.UtcNow.Year.ToString();
            string typeString = type.ToString(); 

            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT nextval('\"{sequenceName}\"')";            

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            var nextValue = (long)(await command.ExecuteScalarAsync())!;

            return $"{typeString}-{warehouseCode}-{currentYear}-{nextValue.ToString().PadLeft(numberLength, '0')}";
        }
        
    }
}