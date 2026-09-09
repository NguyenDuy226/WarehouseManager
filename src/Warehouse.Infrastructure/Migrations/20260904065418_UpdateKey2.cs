using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKey2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockDocumentLines_MaterialId",
                schema: "stock",
                table: "StockDocumentLines",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockDocumentLines_Materials_MaterialId",
                schema: "stock",
                table: "StockDocumentLines",
                column: "MaterialId",
                principalSchema: "warehouse",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockDocumentLines_Materials_MaterialId",
                schema: "stock",
                table: "StockDocumentLines");

            migrationBuilder.DropIndex(
                name: "IX_StockDocumentLines_MaterialId",
                schema: "stock",
                table: "StockDocumentLines");
        }
    }
}
