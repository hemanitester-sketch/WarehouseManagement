using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Faculty_Members : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "AppWarehouses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppFacultyMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Designation = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Department = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    AssignedWarehouseId = table.Column<Guid>(type: "uuid", nullable: true),
                    Shift = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Qualifications = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_AppFacultyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFacultyMembers_AppWarehouses_AssignedWarehouseId",
                        column: x => x.AssignedWarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_ManagerId",
                table: "AppWarehouses",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppWarehouses_TenantId_ManagerId",
                table: "AppWarehouses",
                columns: new[] { "TenantId", "ManagerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFacultyMembers_AssignedWarehouseId",
                table: "AppFacultyMembers",
                column: "AssignedWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFacultyMembers_TenantId_AssignedWarehouseId",
                table: "AppFacultyMembers",
                columns: new[] { "TenantId", "AssignedWarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppFacultyMembers_TenantId_EmployeeCode",
                table: "AppFacultyMembers",
                columns: new[] { "TenantId", "EmployeeCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppWarehouses_AppFacultyMembers_ManagerId",
                table: "AppWarehouses",
                column: "ManagerId",
                principalTable: "AppFacultyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppWarehouses_AppFacultyMembers_ManagerId",
                table: "AppWarehouses");

            migrationBuilder.DropTable(
                name: "AppFacultyMembers");

            migrationBuilder.DropIndex(
                name: "IX_AppWarehouses_ManagerId",
                table: "AppWarehouses");

            migrationBuilder.DropIndex(
                name: "IX_AppWarehouses_TenantId_ManagerId",
                table: "AppWarehouses");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AppWarehouses");
        }
    }
}
