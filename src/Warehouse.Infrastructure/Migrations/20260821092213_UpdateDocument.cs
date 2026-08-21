using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "stock",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "stock",
                table: "StockBalances");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "stock",
                table: "ApprovalHistories");

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                schema: "stock",
                table: "StockMovements",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                schema: "stock",
                table: "StockMovements",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "stock",
                table: "StockDocuments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ToWarehouseId",
                schema: "stock",
                table: "StockDocuments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalValue",
                schema: "stock",
                table: "StockBalances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                schema: "stock",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                schema: "stock",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropColumn(
                name: "ToWarehouseId",
                schema: "stock",
                table: "StockDocuments");

            migrationBuilder.DropColumn(
                name: "TotalValue",
                schema: "stock",
                table: "StockBalances");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "stock",
                table: "StockMovements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "stock",
                table: "StockDocuments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "stock",
                table: "StockDocuments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "stock",
                table: "StockBalances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "stock",
                table: "ApprovalHistories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
