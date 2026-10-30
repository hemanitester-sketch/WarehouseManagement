using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Cycle_Counts_And_Audits_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCycleCountAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ZoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    CountType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsBlindCount = table.Column<bool>(type: "boolean", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToFacultyId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAuditorName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedByFacultyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalVarianceQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalVarianceValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AppCycleCountAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCycleCountAudits_AppWarehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppCycleCountAuditItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CycleCountAuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotBatchNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SystemQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CountedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscrepancyReason = table.Column<int>(type: "integer", nullable: false),
                    ReasonNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsReCounted = table.Column<bool>(type: "boolean", nullable: false),
                    CountedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCycleCountAuditItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCycleCountAuditItems_AppCycleCountAudits_CycleCountAudit~",
                        column: x => x.CycleCountAuditId,
                        principalTable: "AppCycleCountAudits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCycleCountAuditItems_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCycleCountAuditItems_AppWarehouseLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAuditItems_CycleCountAuditId_ProductId_Locatio~",
                table: "AppCycleCountAuditItems",
                columns: new[] { "CycleCountAuditId", "ProductId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAuditItems_LocationId",
                table: "AppCycleCountAuditItems",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAuditItems_ProductId",
                table: "AppCycleCountAuditItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAudits_AuditNumber",
                table: "AppCycleCountAudits",
                column: "AuditNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAudits_WarehouseId_Status",
                table: "AppCycleCountAudits",
                columns: new[] { "WarehouseId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCycleCountAuditItems");

            migrationBuilder.DropTable(
                name: "AppCycleCountAudits");
        }
    }
}
