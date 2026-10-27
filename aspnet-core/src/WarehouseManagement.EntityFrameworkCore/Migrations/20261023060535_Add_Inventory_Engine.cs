using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Inventory_Engine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppInventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityAllocated = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LotBatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Lpn = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManufactureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppInventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryItems_AppWarehouseLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryItems_AppWarehouseZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "AppWarehouseZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryItems_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LotBatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Lpn = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TransactionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventoryTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_LocationId",
                table: "AppInventoryItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_ProductId",
                table: "AppInventoryItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_LotBatchNumber",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "LotBatchNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_Lpn",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "Lpn" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_ProductId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_SerialNumber",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "SerialNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_WarehouseId_LocationId_ProductId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "WarehouseId", "LocationId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_WarehouseId",
                table: "AppInventoryItems",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_ZoneId",
                table: "AppInventoryItems",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_TenantId_ReferenceNumber",
                table: "AppInventoryTransactions",
                columns: new[] { "TenantId", "ReferenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_TenantId_TransactionTime",
                table: "AppInventoryTransactions",
                columns: new[] { "TenantId", "TransactionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_TenantId_WarehouseId_ProductId",
                table: "AppInventoryTransactions",
                columns: new[] { "TenantId", "WarehouseId", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppInventoryItems");

            migrationBuilder.DropTable(
                name: "AppInventoryTransactions");
        }
    }
}
