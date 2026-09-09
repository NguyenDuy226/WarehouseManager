using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_SupplierId",
                schema: "stock",
                table: "StockDocuments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_WarehouseId",
                schema: "stock",
                table: "StockDocuments",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocuments_Suppliers_SupplierId",
                schema: "stock",
                table: "StockDocuments",
                column: "SupplierId",
                principalSchema: "warehouse",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocuments_Warehouses_WarehouseId",
                schema: "stock",
                table: "StockDocuments",
                column: "WarehouseId",
                principalSchema: "warehouse",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockDocuments_Suppliers_SupplierId",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_StockDocuments_Warehouses_WarehouseId",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocuments_SupplierId",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocuments_WarehouseId",
                schema: "stock",
                table: "StockDocuments");
        }
    }
}
