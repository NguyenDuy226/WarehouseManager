using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKey3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockDocuments_ToWarehouseId",
                schema: "stock",
                table: "StockDocuments",
                column: "ToWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocuments_Warehouses_ToWarehouseId",
                schema: "stock",
                table: "StockDocuments",
                column: "ToWarehouseId",
                principalSchema: "warehouse",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockDocuments_Warehouses_ToWarehouseId",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropIndex(
                name: "IX_StockDocuments_ToWarehouseId",
                schema: "stock",
                table: "StockDocuments");
        }
    }
}
