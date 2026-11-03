using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class Add_Product_Variants_And_Barcode_Engine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppSuppliers_TenantId_IsActive",
                table: "AppSuppliers");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppStockTransferItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppReturnOrderItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WidthCm",
                table: "AppProducts",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "AppProducts",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "VolumeCm3",
                table: "AppProducts",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "LengthCm",
                table: "AppProducts",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HeightCm",
                table: "AppProducts",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerPartNumber",
                table: "AppProducts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "AppProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppOutboundOrderLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LicensePlateId",
                table: "AppInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "AppInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SerialId",
                table: "AppInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LicensePlateId",
                table: "AppInventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LotId",
                table: "AppInventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppInventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SerialId",
                table: "AppInventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppInboundOrderLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductVariantId",
                table: "AppCycleCountAuditItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "AppCustomers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AppLicensePlates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LpnCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    TareWeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxWeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_AppLicensePlates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLicensePlates_AppWarehouseLocations_CurrentLocationId",
                        column: x => x.CurrentLocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppLicensePlates_AppWarehouses_CurrentWarehouseId",
                        column: x => x.CurrentWarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProductAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    IsVariantDefining = table.Column<bool>(type: "boolean", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppProductAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    VariantName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CombinationHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    LengthCm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    WidthCm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductVariants_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppCategoryAttributeTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductAttributeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppCategoryAttributeTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCategoryAttributeTemplates_AppCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AppCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppCategoryAttributeTemplates_AppProductAttributes_ProductA~",
                        column: x => x.ProductAttributeId,
                        principalTable: "AppProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppProductAttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductAttributeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ColorHexCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppProductAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductAttributeValues_AppProductAttributes_ProductAttri~",
                        column: x => x.ProductAttributeId,
                        principalTable: "AppProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppInventoryLots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupplierLotNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CertificateUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppInventoryLots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventoryLots_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventoryLots_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventorySerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CurrentWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentLicensePlateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppInventorySerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventorySerials_AppLicensePlates_CurrentLicensePlateId",
                        column: x => x.CurrentLicensePlateId,
                        principalTable: "AppLicensePlates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppInventorySerials_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventorySerials_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppInventorySerials_AppWarehouseLocations_CurrentLocationId",
                        column: x => x.CurrentLocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppInventorySerials_AppWarehouses_CurrentWarehouseId",
                        column: x => x.CurrentWarehouseId,
                        principalTable: "AppWarehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProductVariantAttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductAttributeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductAttributeValueId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_AppProductVariantAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductVariantAttributeValues_AppProductAttributeValues_~",
                        column: x => x.ProductAttributeValueId,
                        principalTable: "AppProductAttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProductVariantAttributeValues_AppProductAttributes_Produ~",
                        column: x => x.ProductAttributeId,
                        principalTable: "AppProductAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProductVariantAttributeValues_AppProductVariants_Product~",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppLicensePlateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicensePlateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityOnHand = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Uom = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppLicensePlateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateItems_AppInventoryLots_LotId",
                        column: x => x.LotId,
                        principalTable: "AppInventoryLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateItems_AppLicensePlates_LicensePlateId",
                        column: x => x.LicensePlateId,
                        principalTable: "AppLicensePlates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateItems_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppBarcodeIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    NormalizedBarcode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BarcodeTargetType = table.Column<int>(type: "integer", nullable: false),
                    BarcodeKind = table.Column<int>(type: "integer", nullable: false),
                    Symbology = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicensePlateId = table.Column<Guid>(type: "uuid", nullable: true),
                    WarehouseLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryLotId = table.Column<Guid>(type: "uuid", nullable: true),
                    InventorySerialId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppBarcodeIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppBarcodeIdentities_AppInventoryLots_InventoryLotId",
                        column: x => x.InventoryLotId,
                        principalTable: "AppInventoryLots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppBarcodeIdentities_AppInventorySerials_InventorySerialId",
                        column: x => x.InventorySerialId,
                        principalTable: "AppInventorySerials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppBarcodeIdentities_AppLicensePlates_LicensePlateId",
                        column: x => x.LicensePlateId,
                        principalTable: "AppLicensePlates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppBarcodeIdentities_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppBarcodeIdentities_AppWarehouseLocations_WarehouseLocatio~",
                        column: x => x.WarehouseLocationId,
                        principalTable: "AppWarehouseLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppInventorySerialHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SerialId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    FromLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromLicensePlateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToLicensePlateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceDocumentNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EventTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppInventorySerialHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppInventorySerialHistories_AppInventorySerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "AppInventorySerials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppLicensePlateSerials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicensePlateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppLicensePlateSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateSerials_AppInventorySerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "AppInventorySerials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateSerials_AppLicensePlates_LicensePlateId",
                        column: x => x.LicensePlateId,
                        principalTable: "AppLicensePlates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppLicensePlateSerials_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProductBarcodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    BarcodeIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagingLevel = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    PrintFormat = table.Column<int>(type: "integer", nullable: false),
                    PrintedCount = table.Column<int>(type: "integer", nullable: false),
                    LastPrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_AppProductBarcodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProductBarcodes_AppBarcodeIdentities_BarcodeIdentityId",
                        column: x => x.BarcodeIdentityId,
                        principalTable: "AppBarcodeIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProductBarcodes_AppProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppProductBarcodes_AppProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AppProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppSupplierBarcodeMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarcodeIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierPartNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppSupplierBarcodeMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSupplierBarcodeMappings_AppBarcodeIdentities_BarcodeIden~",
                        column: x => x.BarcodeIdentityId,
                        principalTable: "AppBarcodeIdentities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSupplierBarcodeMappings_AppProductVariants_ProductVarian~",
                        column: x => x.ProductVariantId,
                        principalTable: "AppProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppSupplierBarcodeMappings_AppSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "AppSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_TenantId_Name",
                table: "AppSuppliers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransferItems_ProductVariantId",
                table: "AppStockTransferItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStockTransferItems_StockTransferId_ProductVariantId",
                table: "AppStockTransferItems",
                columns: new[] { "StockTransferId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrderItems_ProductVariantId",
                table: "AppReturnOrderItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppReturnOrderItems_ReturnOrderId_ProductVariantId",
                table: "AppReturnOrderItems",
                columns: new[] { "ReturnOrderId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProducts_TenantId_ProductType",
                table: "AppProducts",
                columns: new[] { "TenantId", "ProductType" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrderLines_ProductVariantId",
                table: "AppOutboundOrderLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppOutboundOrderLines_TenantId_OutboundOrderId_ProductVaria~",
                table: "AppOutboundOrderLines",
                columns: new[] { "TenantId", "OutboundOrderId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_FromLocationId",
                table: "AppInventoryTransactions",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_LicensePlateId",
                table: "AppInventoryTransactions",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_LotId",
                table: "AppInventoryTransactions",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_ProductId",
                table: "AppInventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_ProductVariantId",
                table: "AppInventoryTransactions",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_SerialId",
                table: "AppInventoryTransactions",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_TenantId_WarehouseId_ProductVarian~",
                table: "AppInventoryTransactions",
                columns: new[] { "TenantId", "WarehouseId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_ToLocationId",
                table: "AppInventoryTransactions",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryTransactions_WarehouseId",
                table: "AppInventoryTransactions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_LicensePlateId",
                table: "AppInventoryItems",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_LotId",
                table: "AppInventoryItems",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_ProductVariantId",
                table: "AppInventoryItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_SerialId",
                table: "AppInventoryItems",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_LicensePlateId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "LicensePlateId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_LotId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "LotId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_ProductVariantId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_SerialId",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "SerialId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryItems_TenantId_WarehouseId_LocationId_ProductVa~",
                table: "AppInventoryItems",
                columns: new[] { "TenantId", "WarehouseId", "LocationId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrderLines_ProductVariantId",
                table: "AppInboundOrderLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInboundOrderLines_TenantId_InboundOrderId_ProductVariant~",
                table: "AppInboundOrderLines",
                columns: new[] { "TenantId", "InboundOrderId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAuditItems_CycleCountAuditId_ProductVariantId_~",
                table: "AppCycleCountAuditItems",
                columns: new[] { "CycleCountAuditId", "ProductVariantId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppCycleCountAuditItems_ProductVariantId",
                table: "AppCycleCountAuditItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_InventoryLotId",
                table: "AppBarcodeIdentities",
                column: "InventoryLotId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_InventorySerialId",
                table: "AppBarcodeIdentities",
                column: "InventorySerialId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_LicensePlateId",
                table: "AppBarcodeIdentities",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_ProductVariantId",
                table: "AppBarcodeIdentities",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_InventoryLotId",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "InventoryLotId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_InventorySerialId",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "InventorySerialId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_LicensePlateId",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "LicensePlateId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_NormalizedBarcode",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "NormalizedBarcode" },
                unique: true,
                filter: "\"IsActive\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_ProductVariantId",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_TenantId_WarehouseLocationId",
                table: "AppBarcodeIdentities",
                columns: new[] { "TenantId", "WarehouseLocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBarcodeIdentities_WarehouseLocationId",
                table: "AppBarcodeIdentities",
                column: "WarehouseLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCategoryAttributeTemplates_CategoryId",
                table: "AppCategoryAttributeTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCategoryAttributeTemplates_ProductAttributeId",
                table: "AppCategoryAttributeTemplates",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppCategoryAttributeTemplates_TenantId_CategoryId_ProductAt~",
                table: "AppCategoryAttributeTemplates",
                columns: new[] { "TenantId", "CategoryId", "ProductAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryLots_ProductId",
                table: "AppInventoryLots",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryLots_ProductVariantId",
                table: "AppInventoryLots",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventoryLots_TenantId_ProductVariantId_LotNumber",
                table: "AppInventoryLots",
                columns: new[] { "TenantId", "ProductVariantId", "LotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerialHistories_SerialId",
                table: "AppInventorySerialHistories",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerialHistories_TenantId_EventTime",
                table: "AppInventorySerialHistories",
                columns: new[] { "TenantId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerialHistories_TenantId_SerialId",
                table: "AppInventorySerialHistories",
                columns: new[] { "TenantId", "SerialId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_CurrentLicensePlateId",
                table: "AppInventorySerials",
                column: "CurrentLicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_CurrentLocationId",
                table: "AppInventorySerials",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_CurrentWarehouseId",
                table: "AppInventorySerials",
                column: "CurrentWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_ProductId",
                table: "AppInventorySerials",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_ProductVariantId",
                table: "AppInventorySerials",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppInventorySerials_TenantId_ProductVariantId_SerialNumber",
                table: "AppInventorySerials",
                columns: new[] { "TenantId", "ProductVariantId", "SerialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateItems_LicensePlateId",
                table: "AppLicensePlateItems",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateItems_LotId",
                table: "AppLicensePlateItems",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateItems_ProductVariantId",
                table: "AppLicensePlateItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateItems_TenantId_LicensePlateId_ProductVariant~",
                table: "AppLicensePlateItems",
                columns: new[] { "TenantId", "LicensePlateId", "ProductVariantId", "LotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlates_CurrentLocationId",
                table: "AppLicensePlates",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlates_CurrentWarehouseId",
                table: "AppLicensePlates",
                column: "CurrentWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlates_TenantId_LpnCode",
                table: "AppLicensePlates",
                columns: new[] { "TenantId", "LpnCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateSerials_LicensePlateId",
                table: "AppLicensePlateSerials",
                column: "LicensePlateId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateSerials_ProductVariantId",
                table: "AppLicensePlateSerials",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateSerials_SerialId",
                table: "AppLicensePlateSerials",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateSerials_TenantId_LicensePlateId",
                table: "AppLicensePlateSerials",
                columns: new[] { "TenantId", "LicensePlateId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppLicensePlateSerials_TenantId_SerialId",
                table: "AppLicensePlateSerials",
                columns: new[] { "TenantId", "SerialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProductAttributes_TenantId_Code",
                table: "AppProductAttributes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProductAttributeValues_ProductAttributeId",
                table: "AppProductAttributeValues",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductAttributeValues_TenantId_ProductAttributeId_Value",
                table: "AppProductAttributeValues",
                columns: new[] { "TenantId", "ProductAttributeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBarcodes_BarcodeIdentityId",
                table: "AppProductBarcodes",
                column: "BarcodeIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBarcodes_ProductId",
                table: "AppProductBarcodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBarcodes_ProductVariantId",
                table: "AppProductBarcodes",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBarcodes_TenantId_BarcodeIdentityId",
                table: "AppProductBarcodes",
                columns: new[] { "TenantId", "BarcodeIdentityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProductBarcodes_TenantId_ProductVariantId",
                table: "AppProductBarcodes",
                columns: new[] { "TenantId", "ProductVariantId" },
                unique: true,
                filter: "\"IsPrimary\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariantAttributeValues_ProductAttributeId",
                table: "AppProductVariantAttributeValues",
                column: "ProductAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariantAttributeValues_ProductAttributeValueId",
                table: "AppProductVariantAttributeValues",
                column: "ProductAttributeValueId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariantAttributeValues_ProductVariantId",
                table: "AppProductVariantAttributeValues",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariantAttributeValues_TenantId_ProductVariantId_~",
                table: "AppProductVariantAttributeValues",
                columns: new[] { "TenantId", "ProductVariantId", "ProductAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariants_ProductId",
                table: "AppProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariants_TenantId_ProductId",
                table: "AppProductVariants",
                columns: new[] { "TenantId", "ProductId" },
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariants_TenantId_ProductId_CombinationHash",
                table: "AppProductVariants",
                columns: new[] { "TenantId", "ProductId", "CombinationHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProductVariants_TenantId_VariantSku",
                table: "AppProductVariants",
                columns: new[] { "TenantId", "VariantSku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSupplierBarcodeMappings_BarcodeIdentityId",
                table: "AppSupplierBarcodeMappings",
                column: "BarcodeIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSupplierBarcodeMappings_ProductVariantId",
                table: "AppSupplierBarcodeMappings",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSupplierBarcodeMappings_SupplierId",
                table: "AppSupplierBarcodeMappings",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSupplierBarcodeMappings_TenantId_ProductVariantId",
                table: "AppSupplierBarcodeMappings",
                columns: new[] { "TenantId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppSupplierBarcodeMappings_TenantId_SupplierId_BarcodeIdent~",
                table: "AppSupplierBarcodeMappings",
                columns: new[] { "TenantId", "SupplierId", "BarcodeIdentityId" },
                unique: true,
                filter: "\"IsActive\" = TRUE");

            migrationBuilder.AddForeignKey(
                name: "FK_AppCycleCountAuditItems_AppProductVariants_ProductVariantId",
                table: "AppCycleCountAuditItems",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInboundOrderLines_AppProductVariants_ProductVariantId",
                table: "AppInboundOrderLines",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryItems_AppInventoryLots_LotId",
                table: "AppInventoryItems",
                column: "LotId",
                principalTable: "AppInventoryLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryItems_AppInventorySerials_SerialId",
                table: "AppInventoryItems",
                column: "SerialId",
                principalTable: "AppInventorySerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryItems_AppLicensePlates_LicensePlateId",
                table: "AppInventoryItems",
                column: "LicensePlateId",
                principalTable: "AppLicensePlates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryItems_AppProductVariants_ProductVariantId",
                table: "AppInventoryItems",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppInventoryLots_LotId",
                table: "AppInventoryTransactions",
                column: "LotId",
                principalTable: "AppInventoryLots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppInventorySerials_SerialId",
                table: "AppInventoryTransactions",
                column: "SerialId",
                principalTable: "AppInventorySerials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppLicensePlates_LicensePlateId",
                table: "AppInventoryTransactions",
                column: "LicensePlateId",
                principalTable: "AppLicensePlates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppProductVariants_ProductVariantId",
                table: "AppInventoryTransactions",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppProducts_ProductId",
                table: "AppInventoryTransactions",
                column: "ProductId",
                principalTable: "AppProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouseLocations_FromLocation~",
                table: "AppInventoryTransactions",
                column: "FromLocationId",
                principalTable: "AppWarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouseLocations_ToLocationId",
                table: "AppInventoryTransactions",
                column: "ToLocationId",
                principalTable: "AppWarehouseLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouses_WarehouseId",
                table: "AppInventoryTransactions",
                column: "WarehouseId",
                principalTable: "AppWarehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppOutboundOrderLines_AppProductVariants_ProductVariantId",
                table: "AppOutboundOrderLines",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppReturnOrderItems_AppProductVariants_ProductVariantId",
                table: "AppReturnOrderItems",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStockTransferItems_AppProductVariants_ProductVariantId",
                table: "AppStockTransferItems",
                column: "ProductVariantId",
                principalTable: "AppProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppCycleCountAuditItems_AppProductVariants_ProductVariantId",
                table: "AppCycleCountAuditItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInboundOrderLines_AppProductVariants_ProductVariantId",
                table: "AppInboundOrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryItems_AppInventoryLots_LotId",
                table: "AppInventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryItems_AppInventorySerials_SerialId",
                table: "AppInventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryItems_AppLicensePlates_LicensePlateId",
                table: "AppInventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryItems_AppProductVariants_ProductVariantId",
                table: "AppInventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppInventoryLots_LotId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppInventorySerials_SerialId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppLicensePlates_LicensePlateId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppProductVariants_ProductVariantId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppProducts_ProductId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouseLocations_FromLocation~",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouseLocations_ToLocationId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppInventoryTransactions_AppWarehouses_WarehouseId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_AppOutboundOrderLines_AppProductVariants_ProductVariantId",
                table: "AppOutboundOrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_AppReturnOrderItems_AppProductVariants_ProductVariantId",
                table: "AppReturnOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStockTransferItems_AppProductVariants_ProductVariantId",
                table: "AppStockTransferItems");

            migrationBuilder.DropTable(
                name: "AppCategoryAttributeTemplates");

            migrationBuilder.DropTable(
                name: "AppInventorySerialHistories");

            migrationBuilder.DropTable(
                name: "AppLicensePlateItems");

            migrationBuilder.DropTable(
                name: "AppLicensePlateSerials");

            migrationBuilder.DropTable(
                name: "AppProductBarcodes");

            migrationBuilder.DropTable(
                name: "AppProductVariantAttributeValues");

            migrationBuilder.DropTable(
                name: "AppSupplierBarcodeMappings");

            migrationBuilder.DropTable(
                name: "AppProductAttributeValues");

            migrationBuilder.DropTable(
                name: "AppBarcodeIdentities");

            migrationBuilder.DropTable(
                name: "AppProductAttributes");

            migrationBuilder.DropTable(
                name: "AppInventoryLots");

            migrationBuilder.DropTable(
                name: "AppInventorySerials");

            migrationBuilder.DropTable(
                name: "AppLicensePlates");

            migrationBuilder.DropTable(
                name: "AppProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_AppSuppliers_TenantId_Name",
                table: "AppSuppliers");

            migrationBuilder.DropIndex(
                name: "IX_AppStockTransferItems_ProductVariantId",
                table: "AppStockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_AppStockTransferItems_StockTransferId_ProductVariantId",
                table: "AppStockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_AppReturnOrderItems_ProductVariantId",
                table: "AppReturnOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_AppReturnOrderItems_ReturnOrderId_ProductVariantId",
                table: "AppReturnOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_AppProducts_TenantId_ProductType",
                table: "AppProducts");

            migrationBuilder.DropIndex(
                name: "IX_AppOutboundOrderLines_ProductVariantId",
                table: "AppOutboundOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_AppOutboundOrderLines_TenantId_OutboundOrderId_ProductVaria~",
                table: "AppOutboundOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_FromLocationId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_LicensePlateId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_LotId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_ProductId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_ProductVariantId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_SerialId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_TenantId_WarehouseId_ProductVarian~",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_ToLocationId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryTransactions_WarehouseId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_LicensePlateId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_LotId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_ProductVariantId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_SerialId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_TenantId_LicensePlateId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_TenantId_LotId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_TenantId_ProductVariantId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_TenantId_SerialId",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInventoryItems_TenantId_WarehouseId_LocationId_ProductVa~",
                table: "AppInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_AppInboundOrderLines_ProductVariantId",
                table: "AppInboundOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_AppInboundOrderLines_TenantId_InboundOrderId_ProductVariant~",
                table: "AppInboundOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_AppCycleCountAuditItems_CycleCountAuditId_ProductVariantId_~",
                table: "AppCycleCountAuditItems");

            migrationBuilder.DropIndex(
                name: "IX_AppCycleCountAuditItems_ProductVariantId",
                table: "AppCycleCountAuditItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppStockTransferItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppReturnOrderItems");

            migrationBuilder.DropColumn(
                name: "ManufacturerPartNumber",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "AppProducts");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppOutboundOrderLines");

            migrationBuilder.DropColumn(
                name: "LicensePlateId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "SerialId",
                table: "AppInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "LicensePlateId",
                table: "AppInventoryItems");

            migrationBuilder.DropColumn(
                name: "LotId",
                table: "AppInventoryItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppInventoryItems");

            migrationBuilder.DropColumn(
                name: "SerialId",
                table: "AppInventoryItems");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppInboundOrderLines");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "AppCycleCountAuditItems");

            migrationBuilder.AlterColumn<double>(
                name: "WidthCm",
                table: "AppProducts",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "WeightKg",
                table: "AppProducts",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "VolumeCm3",
                table: "AppProducts",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "LengthCm",
                table: "AppProducts",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "HeightCm",
                table: "AppProducts",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "AppCustomers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSuppliers_TenantId_IsActive",
                table: "AppSuppliers",
                columns: new[] { "TenantId", "IsActive" });
        }
    }
}
