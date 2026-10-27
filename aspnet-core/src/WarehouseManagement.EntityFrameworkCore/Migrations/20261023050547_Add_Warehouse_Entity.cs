using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Warehouse_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWarehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    State = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    TotalAreaSqFt = table.Column<double>(type: "double precision", nullable: false),
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
                    table.PrimaryKey("PK_AppWarehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_TenantId_Code",
                table: "AppWarehouses",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWarehouses");
        }
    }
}
