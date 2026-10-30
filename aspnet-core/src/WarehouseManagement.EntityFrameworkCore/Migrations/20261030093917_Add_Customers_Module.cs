using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Customers_Module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CustomerType = table.Column<int>(type: "integer", nullable: false),
                    Industry = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    AlternativePhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    PanNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BillingAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ShippingAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    State = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PaymentTerms = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentOutstanding = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceTier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeliveryDockHours = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: true),
                    TotalOrdersPlaced = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AppCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_Code",
                table: "AppCustomers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_CustomerType",
                table: "AppCustomers",
                columns: new[] { "TenantId", "CustomerType" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_IsActive",
                table: "AppCustomers",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCustomers_TenantId_Status",
                table: "AppCustomers",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCustomers");
        }
    }
}
