using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Inbound_Operations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppInboundOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SupplierInvoiceNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExpectedArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppInboundOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInboundOrders_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInboundOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboundOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInboundOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInboundOrderLines_AppInboundOrders_InboundOrderId",
                        column: x => x.InboundOrderId,
                        principalTable: "AppInboundOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppInboundOrderLines_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrderLines_InboundOrderId",
                table: "AppInboundOrderLines",
                column: "InboundOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrderLines_ProductId",
                table: "AppInboundOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrderLines_TenantId_InboundOrderId_ProductId",
                table: "AppInboundOrderLines",
                columns: new[] { "TenantId", "InboundOrderId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrders_TenantId_OrderNumber",
                table: "AppInboundOrders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrders_TenantId_Status",
                table: "AppInboundOrders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrders_TenantId_WarehouseId",
                table: "AppInboundOrders",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrders_WarehouseId",
                table: "AppInboundOrders",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppInboundOrderLines");

            migrationBuilder.DropTable(
                name: "AppInboundOrders");
        }
    }
}
