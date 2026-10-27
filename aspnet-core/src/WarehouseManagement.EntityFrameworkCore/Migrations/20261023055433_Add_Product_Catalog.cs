using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Product_Catalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Brand = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Uom = table.Column<int>(type: "integer", nullable: false),
                    IsLotTracked = table.Column<bool>(type: "boolean", nullable: false),
                    IsExpiryTracked = table.Column<bool>(type: "boolean", nullable: false),
                    IsSerialTracked = table.Column<bool>(type: "boolean", nullable: false),
                    WeightKg = table.Column<double>(type: "double precision", nullable: false),
                    VolumeCm3 = table.Column<double>(type: "double precision", nullable: false),
                    LengthCm = table.Column<double>(type: "double precision", nullable: true),
                    WidthCm = table.Column<double>(type: "double precision", nullable: true),
                    HeightCm = table.Column<double>(type: "double precision", nullable: true),
                    RequiredStorageType = table.Column<int>(type: "integer", nullable: false),
                    TemperatureMinCelsius = table.Column<double>(type: "double precision", nullable: true),
                    TemperatureMaxCelsius = table.Column<double>(type: "double precision", nullable: true),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    TargetStockLevel = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_Barcode",
                table: "AppProducts",
                columns: new[] { "TenantId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_Category",
                table: "AppProducts",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_Sku",
                table: "AppProducts",
                columns: new[] { "TenantId", "Sku" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProducts");
        }
    }
}
