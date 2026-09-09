using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stock");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateSequence(
                name: "MaterialCategoryCodeSeq");

            migrationBuilder.CreateSequence(
                name: "MaterialCodeSeq");

            migrationBuilder.CreateSequence(
                name: "StockDocumentSeq");

            migrationBuilder.CreateSequence(
                name: "SupplierCodeSeq");

            migrationBuilder.CreateSequence(
                name: "UnitOfMeasureCodeSeq");

            migrationBuilder.CreateSequence(
                name: "UserCodeSeq");

            migrationBuilder.CreateSequence(
                name: "WarehouseCodeSeq");

            migrationBuilder.CreateTable(
                name: "AuthAuditLogs",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityAuditLogs",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    PrimaryKey = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialCategories",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockDocuments",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Receiver = table.Column<string>(type: "text", nullable: true),
                    IssueDocument_ExternalDocumentNo = table.Column<string>(type: "text", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalDocumentNo = table.Column<string>(type: "text", nullable: true),
                    OriginalDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToWarehouseId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    MovementTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TaxCode = table.Column<string>(type: "text", nullable: false),
                    Addres = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionReasons",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasures",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Manager = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AppRoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_AppRoleId",
                        column: x => x.AppRoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalHistories",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalHistories_StockDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "stock",
                        principalTable: "StockDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockDocumentLines",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(20,4)", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockDocumentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockDocumentLines_StockDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "stock",
                        principalTable: "StockDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MininumStock = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_MaterialCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "warehouse",
                        principalTable: "MaterialCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Materials_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalSchema: "warehouse",
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistories",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistories_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshTokenValue = table.Column<string>(type: "text", nullable: false),
                    Expire = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Create = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehousePermissions",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<string>(type: "text", nullable: false),
                    WarehouseId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehousePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehousePermissions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehousePermissions_Warehouses_WarehouseId1",
                        column: x => x.WarehouseId1,
                        principalSchema: "warehouse",
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false),
                    MovingAveragePrice = table.Column<decimal>(type: "numeric(20,4)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    WarehouseEntityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockBalances_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalSchema: "warehouse",
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockBalances_Warehouses_WarehouseEntityId",
                        column: x => x.WarehouseEntityId,
                        principalSchema: "warehouse",
                        principalTable: "Warehouses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_DocumentId",
                schema: "stock",
                table: "ApprovalHistories",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAuditLogs_CreateAt",
                schema: "audit",
                table: "EntityAuditLogs",
                column: "CreateAt");

            migrationBuilder.CreateIndex(
                name: "IX_EntityAuditLogs_TableName_PrimaryKey",
                schema: "audit",
                table: "EntityAuditLogs",
                columns: new[] { "TableName", "PrimaryKey" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityAuditLogs_UserId",
                schema: "audit",
                table: "EntityAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialCategories_Code",
                schema: "warehouse",
                table: "MaterialCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_CategoryId",
                schema: "warehouse",
                table: "Materials",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Code",
                schema: "warehouse",
                table: "Materials",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_UnitOfMeasureId",
                schema: "warehouse",
                table: "Materials",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId",
                schema: "identity",
                table: "PasswordHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_AppUserId",
                schema: "identity",
                table: "RefreshToken",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_MaterialId",
                schema: "stock",
                table: "StockBalances",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_WarehouseEntityId",
                schema: "stock",
                table: "StockBalances",
                column: "WarehouseEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentLines_DocumentId",
                schema: "stock",
                table: "StockDocumentLines",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_Code",
                schema: "stock",
                table: "StockDocuments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WarehouseId_MaterialId",
                schema: "stock",
                table: "StockMovements",
                columns: new[] { "WarehouseId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                schema: "warehouse",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_Code",
                schema: "warehouse",
                table: "UnitOfMeasures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AppRoleId",
                schema: "identity",
                table: "Users",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Code",
                schema: "identity",
                table: "Users",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehousePermissions_UserId",
                schema: "warehouse",
                table: "WarehousePermissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehousePermissions_WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions",
                column: "WarehouseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                schema: "warehouse",
                table: "Warehouses",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalHistories",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "AuthAuditLogs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "EntityAuditLogs",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "PasswordHistories",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "StockBalances",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "StockDocumentLines",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "StockMovements",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "TransactionReasons",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "WarehousePermissions",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "Materials",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "StockDocuments",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Warehouses",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "MaterialCategories",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "identity");

            migrationBuilder.DropSequence(
                name: "MaterialCategoryCodeSeq");

            migrationBuilder.DropSequence(
                name: "MaterialCodeSeq");

            migrationBuilder.DropSequence(
                name: "StockDocumentSeq");

            migrationBuilder.DropSequence(
                name: "SupplierCodeSeq");

            migrationBuilder.DropSequence(
                name: "UnitOfMeasureCodeSeq");

            migrationBuilder.DropSequence(
                name: "UserCodeSeq");

            migrationBuilder.DropSequence(
                name: "WarehouseCodeSeq");
        }
    }
}
