using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Zones_And_Locations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWarehouseZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StorageType = table.Column<int>(type: "integer", nullable: false),
                    TemperatureMinCelsius = table.Column<double>(type: "double precision", nullable: true),
                    TemperatureMaxCelsius = table.Column<double>(type: "double precision", nullable: true),
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
                    table.PrimaryKey("PK_AppWarehouseZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWarehouseZones_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppWarehouseLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Aisle = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Rack = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Shelf = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Bin = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LocationType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MaxWeightKg = table.Column<double>(type: "double precision", nullable: false),
                    MaxVolumeCm3 = table.Column<double>(type: "double precision", nullable: false),
                    LengthCm = table.Column<double>(type: "double precision", nullable: true),
                    WidthCm = table.Column<double>(type: "double precision", nullable: true),
                    HeightCm = table.Column<double>(type: "double precision", nullable: true),
                    IsPickFace = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppWarehouseLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppWarehouseLocations_AppWarehouseZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "AppWarehouseZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppWarehouseLocations_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseLocations_TenantId_Barcode",
                table: "AppWarehouseLocations",
                columns: new[] { "TenantId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseLocations_TenantId_WarehouseId_ZoneId",
                table: "AppWarehouseLocations",
                columns: new[] { "TenantId", "WarehouseId", "ZoneId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseLocations_WarehouseId",
                table: "AppWarehouseLocations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseLocations_ZoneId",
                table: "AppWarehouseLocations",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseZones_TenantId_WarehouseId_Code",
                table: "AppWarehouseZones",
                columns: new[] { "TenantId", "WarehouseId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouseZones_WarehouseId",
                table: "AppWarehouseZones",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWarehouseLocations");

            migrationBuilder.DropTable(
                name: "AppWarehouseZones");
        }
    }
}
