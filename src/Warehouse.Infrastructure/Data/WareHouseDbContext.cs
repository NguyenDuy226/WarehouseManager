using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure.Data
{
    public class WarehouseDbContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public DbSet<Material> Materials { get; set; }
        public DbSet<MaterialCategory> MaterialCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<TransactionReason> TransactionReasons { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasures { get; set; }
        public DbSet<WarehouseEntity> WarehouseEntities { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<WarehousePermission> WarehousePermissions { get; set; }
        public DbSet<EntityAuditLog> EntityAuditLogs { get; set; }
        public DbSet<AuthAuditLog> AuthAuditLogs { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        public DbSet<StockDocument> StockDocuments { get; set; }
        public DbSet<StockDocumentLine> StockDocumentLines { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<StockBalance> StockBalances { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistories { get; set; }
    
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //warehouse schema
            builder.Entity<Material>()
                .ToTable("Materials", "warehouse");

            builder.Entity<MaterialCategory>()
                .ToTable("MaterialCategories", "warehouse");

            builder.Entity<Supplier>()
                .ToTable("Suppliers", "warehouse");

            builder.Entity<TransactionReason>()
                .ToTable("TransactionReasons", "warehouse");

            builder.Entity<UnitOfMeasure>()
                .ToTable("UnitOfMeasures", "warehouse");

            builder.Entity<WarehouseEntity>()
                .ToTable("Warehouses", "warehouse");

            builder.Entity<WarehousePermission>()
                .ToTable("WarehousePermissions", "warehouse");

            //identity schema
            builder.Entity<AppUser>()
                .ToTable("Users", "identity");

            builder.Entity<RefreshToken>()
                .ToTable("RefreshToken", "identity");

            builder.Entity<AppRole>()
                .ToTable("Roles", "identity");

            builder.Entity<IdentityUserRole<Guid>>()
                .ToTable("UserRoles", "identity");

            builder.Entity<IdentityUserClaim<Guid>>()
                .ToTable("UserClaims", "identity");

            builder.Entity<IdentityUserLogin<Guid>>()
                .ToTable("UserLogins", "identity");

            builder.Entity<IdentityUserToken<Guid>>()
                .ToTable("UserTokens", "identity");

            builder.Entity<IdentityRoleClaim<Guid>>()
                .ToTable("RoleClaims", "identity");

            builder.Entity<PasswordHistory>()
                .ToTable("PasswordHistories", "identity");
            //audit schema
            builder.Entity<EntityAuditLog>(entity =>
            {
                entity.ToTable("EntityAuditLogs", "audit"); 
                entity.HasIndex(e => e.CreateAt);
                entity.HasIndex(e => new { e.TableName, e.PrimaryKey });
                entity.HasIndex(e => e.UserId);
            });
            builder.Entity<AuthAuditLog>()
                .ToTable("AuthAuditLogs", "audit");
            //sequense
            builder.HasSequence<long>("WarehouseCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<long>("MaterialCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<long>("MaterialCategoryCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<long>("SupplierCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<long>("UnitOfMeasureCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<long>("UserCodeSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            //stock sequence
            builder.HasSequence<long>("StockDocumentSeq")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.Entity<AppUser>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<Material>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<MaterialCategory>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<Supplier>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<UnitOfMeasure>()
                .HasIndex(w => w.Code)
                .IsUnique();

            builder.Entity<WarehouseEntity>()
                .HasIndex(w => w.Code)
                .IsUnique();
            
            //stock schema
            builder.Entity<StockDocument>(entity =>
            {
                entity.ToTable("StockDocuments", "stock");
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);                
                entity.HasMany(d => d.Lines)
                      .WithOne(l => l.Document)
                      .HasForeignKey(l => l.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<StockDocumentLine>(entity =>
            {
                entity.ToTable("StockDocumentLines", "stock");
                entity.Property(e => e.Quantity).HasColumnType("numeric(18,4)");
                entity.Property(e => e.UnitPrice).HasColumnType("numeric(20,4)");
                entity.HasQueryFilter(e => !e.IsRemoved);
            });

            builder.Entity<StockMovement>(entity =>
            {
                entity.ToTable("StockMovements", "stock");
                entity.Property(e => e.MovementQuantity).HasColumnType("numeric(18,4)");
                entity.HasIndex(e => new { e.WarehouseId, e.MaterialId });
            });

            builder.Entity<StockBalance>(entity =>
            {
                entity.ToTable("StockBalances", "stock");
                entity.HasKey(e => new { e.WarehouseId, e.MaterialId });
                entity.Property(e => e.TotalQuantity).HasColumnType("numeric(18,4)");
                entity.Property(e => e.MovingAveragePrice).HasColumnType("numeric(20,4)");
                entity.Property(e => e.RowVersion).IsRowVersion().IsConcurrencyToken();
            });

            builder.Entity<ApprovalHistory>(entity =>
            {
                entity.ToTable("ApprovalHistories", "stock");
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(500);
            });

            //filter
            builder.Entity<WarehouseEntity>().HasQueryFilter(x => !x.IsRemoved);
            builder.Entity<Material>().HasQueryFilter(x => !x.IsRemoved);
            builder.Entity<Supplier>().HasQueryFilter(x => !x.IsRemoved);
            builder.Entity<MaterialCategory>().HasQueryFilter(x => !x.IsRemoved);
            builder.Entity<UnitOfMeasure>().HasQueryFilter(x => !x.IsRemoved);
                            
            }
        // public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        // {
        //     CreateAuditLog();
        //     return await base.SaveChangesAsync(cancellationToken);
        // }   
        // public override int SaveChanges()
        // {
        //     CreateAuditLog();
        //     return base.SaveChanges();
        // }
        // private void CreateAuditLog()
        // {
        //     ChangeTracker.DetectChanges();
            
        //     foreach (var entry in ChangeTracker.Entries())
        //     {
        //         if (entry.State == EntityState.Deleted && entry.Entity.GetType().GetProperty("IsRemoved") != null)
        //         {
        //             entry.State = EntityState.Modified;
        //             entry.CurrentValues["IsRemoved"] = true;
        //         }
        //     }

        //     var audit = new List<EntityAuditLog>();
        //     var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     Guid? currentUserId = Guid.TryParse(userId, out var parsedId) ? parsedId : null;
        //     var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        //     foreach(var item in ChangeTracker.Entries())
        //     {
        //         if(item.Entity is EntityAuditLog || item.State == EntityState.Detached ||item.State == EntityState.Unchanged) continue;
        //         var auditLog = new EntityAuditLog
        //         {
        //             Id = Guid.NewGuid(),
        //             UserId = currentUserId,
        //             ActionType = item.State.ToString(),
        //             CreateAt = DateTime.UtcNow,
        //             IpAddress = ipAddress,
        //             TableName = item.Metadata.GetTableName() ?? item.Entity.GetType().Name,
        //         };
        //         var oldValues = new Dictionary<string, object?>();
        //         var newValues = new Dictionary<string, object?>();
        //         var pkValues = new List<string>();
                
        //         foreach(var property in item.Properties)
        //         {
        //             string propertyName = property.Metadata.Name;
        //             if (property.Metadata.IsPrimaryKey())
        //             {
        //                 pkValues.Add(property.CurrentValue?.ToString() ?? string.Empty);
        //             }
        //             switch (item.State)
        //             {
        //                 case EntityState.Added:
        //                     newValues[propertyName] = property.CurrentValue;
        //                     break;

        //                 case EntityState.Deleted:
        //                     oldValues[propertyName] = property.OriginalValue;
        //                     break;
        //                 case EntityState.Modified:
        //                     if (property.IsModified)
        //                     {
        //                         oldValues[propertyName] = property.OriginalValue;
        //                         newValues[propertyName] = property.CurrentValue;
        //                     }
        //                     break;
        //             }
        //         }
        //         auditLog.PrimaryKey = string.Join("|", pkValues);
        //         auditLog.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
        //         auditLog.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);
        //         audit.Add(auditLog);
        //     }
        //     if (audit.Any())
        //     {
        //         EntityAuditLogs.AddRange(audit);
        //     }

        //     }
        
        
        }
}