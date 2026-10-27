using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Outbound_Operations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppOutboundOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ShippingAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    State = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredDeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CarrierName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppOutboundOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOutboundOrders_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppOutboundOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboundOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    PickedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOutboundOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppOutboundOrderLines_AppOutboundOrders_OutboundOrderId",
                        column: x => x.OutboundOrderId,
                        principalTable: "AppOutboundOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppOutboundOrderLines_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrderLines_OutboundOrderId",
                table: "AppOutboundOrderLines",
                column: "OutboundOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrderLines_ProductId",
                table: "AppOutboundOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrderLines_TenantId_OutboundOrderId_ProductId",
                table: "AppOutboundOrderLines",
                columns: new[] { "TenantId", "OutboundOrderId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrders_TenantId_OrderNumber",
                table: "AppOutboundOrders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrders_TenantId_Priority",
                table: "AppOutboundOrders",
                columns: new[] { "TenantId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrders_TenantId_Status",
                table: "AppOutboundOrders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrders_TenantId_WarehouseId",
                table: "AppOutboundOrders",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrders_WarehouseId",
                table: "AppOutboundOrders",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppOutboundOrderLines");

            migrationBuilder.DropTable(
                name: "AppOutboundOrders");
        }
    }
}
