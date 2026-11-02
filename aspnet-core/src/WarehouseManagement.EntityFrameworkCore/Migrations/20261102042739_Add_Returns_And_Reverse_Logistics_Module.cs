using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Returns_And_Reverse_Logistics_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppReturnOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    RmaNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReturnType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OutboundOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboundOrderNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    InboundOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboundOrderNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InspectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InspectedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TotalQuantityExpected = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalQuantityReceived = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalQuantityRestocked = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalQuantityScrapped = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalRefundOrCreditAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AppReturnOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReturnOrders_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppReturnOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReturnOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpectedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RestockedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ScrappedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitRefundPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReturnReason = table.Column<int>(type: "integer", nullable: false),
                    ConditionGrade = table.Column<int>(type: "integer", nullable: true),
                    ResolutionAction = table.Column<int>(type: "integer", nullable: false),
                    DestinationLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LotBatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    InspectionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppReturnOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppReturnOrderItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppReturnOrderItems_AppReturnOrders_ReturnOrderId",
                        column: x => x.ReturnOrderId,
                        principalTable: "AppReturnOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppReturnOrderItems_AppWarehouseLocations_DestinationLocati~",
                        column: x => x.DestinationLocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrderItems_DestinationLocationId",
                table: "AppReturnOrderItems",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrderItems_ProductId",
                table: "AppReturnOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrderItems_ReturnOrderId_ProductId",
                table: "AppReturnOrderItems",
                columns: new[] { "ReturnOrderId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrders_CustomerId_Status",
                table: "AppReturnOrders",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrders_ReturnDate",
                table: "AppReturnOrders",
                column: "ReturnDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrders_RmaNumber",
                table: "AppReturnOrders",
                column: "RmaNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrders_SupplierId_Status",
                table: "AppReturnOrders",
                columns: new[] { "SupplierId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrders_WarehouseId_Status",
                table: "AppReturnOrders",
                columns: new[] { "WarehouseId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReturnOrderItems");

            migrationBuilder.DropTable(
                name: "AppReturnOrders");
        }
    }
}
