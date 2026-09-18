using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WarehousePermissions_Warehouses_WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions");

            migrationBuilder.DropIndex(
                name: "IX_WarehousePermissions_WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions");

            migrationBuilder.DropColumn(
                name: "WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions");

            migrationBuilder.Sql(@"ALTER TABLE warehouse.""WarehousePermissions"" ALTER COLUMN ""WarehouseId"" TYPE uuid USING ""WarehouseId""::uuid;");
            
            migrationBuilder.CreateIndex(
                name: "IX_WarehousePermissions_WarehouseId",
                schema: "warehouse",
                table: "WarehousePermissions",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehousePermissions_Warehouses_WarehouseId",
                schema: "warehouse",
                table: "WarehousePermissions",
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
                name: "FK_WarehousePermissions_Warehouses_WarehouseId",
                schema: "warehouse",
                table: "WarehousePermissions");

            migrationBuilder.DropIndex(
                name: "IX_WarehousePermissions_WarehouseId",
                schema: "warehouse",
                table: "WarehousePermissions");

            migrationBuilder.AlterColumn<string>(
                name: "WarehouseId",
                schema: "warehouse",
                table: "WarehousePermissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehousePermissions_WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions",
                column: "WarehouseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_WarehousePermissions_Warehouses_WarehouseId1",
                schema: "warehouse",
                table: "WarehousePermissions",
                column: "WarehouseId1",
                principalSchema: "warehouse",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }
    }
}
