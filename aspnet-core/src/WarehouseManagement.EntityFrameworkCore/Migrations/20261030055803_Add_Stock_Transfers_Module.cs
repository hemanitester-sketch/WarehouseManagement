using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Stock_Transfers_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStockTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VehicleNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DriverName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DriverPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DispatchedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AppStockTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStockTransfers_AppWarehouses_DestinationWarehouseId",
                        column: x => x.DestinationWarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStockTransfers_AppWarehouses_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStockTransferItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityRequested = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityTransferred = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LotBatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Lpn = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStockTransferItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStockTransferItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStockTransferItems_AppStockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalTable: "AppStockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransferItems_ProductId",
                table: "AppStockTransferItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransferItems_StockTransferId_ProductId",
                table: "AppStockTransferItems",
                columns: new[] { "StockTransferId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_DestinationWarehouseId",
                table: "AppStockTransfers",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_SourceWarehouseId",
                table: "AppStockTransfers",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_TenantId_DestinationWarehouseId",
                table: "AppStockTransfers",
                columns: new[] { "TenantId", "DestinationWarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_TenantId_SourceWarehouseId",
                table: "AppStockTransfers",
                columns: new[] { "TenantId", "SourceWarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_TenantId_Status",
                table: "AppStockTransfers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_TenantId_TransferNumber",
                table: "AppStockTransfers",
                columns: new[] { "TenantId", "TransferNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransfers_TenantId_TransferType",
                table: "AppStockTransfers",
                columns: new[] { "TenantId", "TransferType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStockTransferItems");

            migrationBuilder.DropTable(
                name: "AppStockTransfers");
        }
    }
}
