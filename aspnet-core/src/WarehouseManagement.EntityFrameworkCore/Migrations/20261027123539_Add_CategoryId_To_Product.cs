using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_CategoryId_To_Product : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "AppProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_CategoryId",
                table: "AppProducts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_CategoryId",
                table: "AppProducts",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppProducts_AppCategories_CategoryId",
                table: "AppProducts",
                column: "CategoryId",
                principalTable: "AppCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProducts_AppCategories_CategoryId",
                table: "AppProducts");

            migrationBuilder.DropIndex(
                name: "IX_AppProducts_CategoryId",
                table: "AppProducts");

            migrationBuilder.DropIndex(
                name: "IX_AppProducts_TenantId_CategoryId",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "AppProducts");
        }
    }
}
