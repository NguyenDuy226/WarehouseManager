using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "MaterialCategoryCodeSeq");

            migrationBuilder.DropSequence(
                name: "MaterialCodeSeq");

            migrationBuilder.DropSequence(
                name: "SupplierCodeSeq");

            migrationBuilder.DropSequence(
                name: "UnitOfMeasureCodeSeq");

            migrationBuilder.DropSequence(
                name: "UserCodeSeq");

            migrationBuilder.DropSequence(
                name: "WarehouseCodeSeq");

            migrationBuilder.EnsureSchema(
                name: "stock");

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
                name: "StockBalances",
                schema: "stock",
                columns: table => new
                {
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    MovingAveragePrice = table.Column<decimal>(type: "numeric(20,4)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => new { x.WarehouseId, x.MaterialId });
                });

            migrationBuilder.CreateTable(
                name: "StockDocuments",
                schema: "stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
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
                    MovementTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalHistories_DocumentId",
                schema: "stock",
                table: "ApprovalHistories",
                column: "DocumentId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalHistories",
                schema: "stock");

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
                name: "StockDocuments",
                schema: "stock");

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

            migrationBuilder.CreateSequence<int>(
                name: "MaterialCategoryCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "MaterialCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "SupplierCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "UnitOfMeasureCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "UserCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "WarehouseCodeSeq");
        }
    }
}
