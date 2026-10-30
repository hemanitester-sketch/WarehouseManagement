using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace WarehouseManagement.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class WarehouseManagementDbContext :
    AbpDbContext<WarehouseManagementDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    // Warehouses
    public DbSet<WarehouseManagement.Warehouses.Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseManagement.Warehouses.WarehouseZone> WarehouseZones { get; set; }
    public DbSet<WarehouseManagement.Warehouses.WarehouseLocation> WarehouseLocations { get; set; }

    // Faculty & Staff
    public DbSet<WarehouseManagement.Faculty.FacultyMember> FacultyMembers { get; set; }

    // Suppliers & Vendors
    public DbSet<WarehouseManagement.Suppliers.Supplier> Suppliers { get; set; }

    // Customers & B2B Clients
    public DbSet<WarehouseManagement.Customers.Customer> Customers { get; set; }

    // Products
    public DbSet<WarehouseManagement.Products.Category> Categories { get; set; }
    public DbSet<WarehouseManagement.Products.Product> Products { get; set; }

    // Inventory
    public DbSet<WarehouseManagement.Inventory.InventoryItem> InventoryItems { get; set; }
    public DbSet<WarehouseManagement.Inventory.InventoryTransaction> InventoryTransactions { get; set; }

    // Inbound
    public DbSet<WarehouseManagement.Inbound.InboundOrder> InboundOrders { get; set; }
    public DbSet<WarehouseManagement.Inbound.InboundOrderLine> InboundOrderLines { get; set; }

    // Outbound
    public DbSet<WarehouseManagement.Outbound.OutboundOrder> OutboundOrders { get; set; }
    public DbSet<WarehouseManagement.Outbound.OutboundOrderLine> OutboundOrderLines { get; set; }

    // Stock Transfers & Movement
    public DbSet<WarehouseManagement.Transfers.StockTransfer> StockTransfers { get; set; }
    public DbSet<WarehouseManagement.Transfers.StockTransferItem> StockTransferItems { get; set; }

    // Cycle Counts & Stock Audits
    public DbSet<WarehouseManagement.Audits.CycleCountAudit> CycleCountAudits { get; set; }
    public DbSet<WarehouseManagement.Audits.CycleCountAuditItem> CycleCountAuditItems { get; set; }

    #endregion

    public WarehouseManagementDbContext(DbContextOptions<WarehouseManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<WarehouseManagement.Warehouses.Warehouse>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "Warehouses", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxNameLength);
            b.Property(x => x.Address).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxAddressLength);
            b.Property(x => x.City).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxCityLength);
            b.Property(x => x.State).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxStateLength);
            b.Property(x => x.PostalCode).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxPostalCodeLength);
            b.Property(x => x.Country).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxCountryLength);
            b.Property(x => x.ContactPerson).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxContactNameLength);
            b.Property(x => x.ContactPhone).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxPhoneLength);
            b.Property(x => x.ContactEmail).HasMaxLength(WarehouseManagement.Warehouses.WarehouseConsts.MaxEmailLength);

            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ManagerId });

            b.HasOne<WarehouseManagement.Faculty.FacultyMember>()
                .WithMany()
                .HasForeignKey(x => x.ManagerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<WarehouseManagement.Faculty.FacultyMember>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "FacultyMembers", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxEmployeeCodeLength);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxFullNameLength);
            b.Property(x => x.Designation).IsRequired().HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxDesignationLength);
            b.Property(x => x.Department).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxDepartmentLength);
            b.Property(x => x.Email).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxEmailLength);
            b.Property(x => x.PhoneNumber).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxPhoneLength);
            b.Property(x => x.EmergencyContactName).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxEmergencyContactLength);
            b.Property(x => x.EmergencyContactPhone).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxPhoneLength);
            b.Property(x => x.Shift).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxShiftLength);
            b.Property(x => x.Qualifications).HasMaxLength(WarehouseManagement.Faculty.FacultyConsts.MaxQualificationsLength);

            b.HasIndex(x => new { x.TenantId, x.EmployeeCode }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.AssignedWarehouseId });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.AssignedWarehouseId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<WarehouseManagement.Suppliers.Supplier>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "Suppliers", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxNameLength);
            b.Property(x => x.ContactPerson).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxContactPersonLength);
            b.Property(x => x.Email).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxEmailLength);
            b.Property(x => x.PhoneNumber).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxPhoneLength);
            b.Property(x => x.TaxNumber).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxTaxNumberLength);
            b.Property(x => x.Address).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxAddressLength);
            b.Property(x => x.City).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxCityLength);
            b.Property(x => x.State).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxStateLength);
            b.Property(x => x.PostalCode).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxPostalCodeLength);
            b.Property(x => x.PaymentTerms).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxPaymentTermsLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Suppliers.SupplierConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<WarehouseManagement.Customers.Customer>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "Customers", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxNameLength);
            b.Property(x => x.Industry).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxIndustryLength);
            b.Property(x => x.ContactPerson).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxContactPersonLength);
            b.Property(x => x.Email).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxEmailLength);
            b.Property(x => x.PhoneNumber).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPhoneLength);
            b.Property(x => x.AlternativePhone).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPhoneLength);
            b.Property(x => x.TaxNumber).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxTaxNumberLength);
            b.Property(x => x.PanNumber).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPanNumberLength);
            b.Property(x => x.BillingAddress).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxAddressLength);
            b.Property(x => x.ShippingAddress).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxAddressLength);
            b.Property(x => x.City).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxCityLength);
            b.Property(x => x.State).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxStateLength);
            b.Property(x => x.PostalCode).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPostalCodeLength);
            b.Property(x => x.Country).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxCountryLength);
            b.Property(x => x.PaymentTerms).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPaymentTermsLength);
            b.Property(x => x.CreditLimit).HasPrecision(18, 2);
            b.Property(x => x.CurrentOutstanding).HasPrecision(18, 2);
            b.Property(x => x.TotalRevenue).HasPrecision(18, 2);
            b.Property(x => x.PriceTier).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxPriceTierLength);
            b.Property(x => x.DeliveryDockHours).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxDeliveryDockHoursLength);
            b.Property(x => x.SpecialInstructions).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxSpecialInstructionsLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Customers.CustomerConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.CustomerType });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<WarehouseManagement.Warehouses.WarehouseZone>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "WarehouseZones", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxZoneCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxZoneNameLength);

            b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.Code }).IsUnique();
            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Warehouses.WarehouseLocation>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "WarehouseLocations", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Barcode).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxBarcodeLength);
            b.Property(x => x.Aisle).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxAisleLength);
            b.Property(x => x.Rack).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxRackLength);
            b.Property(x => x.Shelf).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxShelfLength);
            b.Property(x => x.Bin).IsRequired().HasMaxLength(WarehouseManagement.Warehouses.WarehouseLocationConsts.MaxBinLength);

            b.HasIndex(x => new { x.TenantId, x.Barcode }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.ZoneId });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Warehouses.WarehouseZone>()
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Products.Category>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "Categories", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(WarehouseManagement.Products.CategoryConsts.MaxCodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Products.CategoryConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(WarehouseManagement.Products.CategoryConsts.MaxDescriptionLength);

            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ParentCategoryId });

            b.HasOne<WarehouseManagement.Products.Category>()
                .WithMany()
                .HasForeignKey(x => x.ParentCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WarehouseManagement.Products.Product>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "Products", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Sku).IsRequired().HasMaxLength(WarehouseManagement.Products.ProductConsts.MaxSkuLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(WarehouseManagement.Products.ProductConsts.MaxNameLength);
            b.Property(x => x.Barcode).HasMaxLength(WarehouseManagement.Products.ProductConsts.MaxBarcodeLength);
            b.Property(x => x.Description).HasMaxLength(WarehouseManagement.Products.ProductConsts.MaxDescriptionLength);
            b.Property(x => x.Brand).HasMaxLength(WarehouseManagement.Products.ProductConsts.MaxBrandLength);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);

            b.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Barcode });
            b.HasIndex(x => new { x.TenantId, x.Category });
            b.HasIndex(x => new { x.TenantId, x.CategoryId });

            b.HasOne<WarehouseManagement.Products.Category>()
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<WarehouseManagement.Inventory.InventoryItem>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "InventoryItems", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.QuantityOnHand).HasPrecision(18, 4);
            b.Property(x => x.QuantityAllocated).HasPrecision(18, 4);
            b.Property(x => x.LotBatchNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLotBatchNumberLength);
            b.Property(x => x.SerialNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxSerialNumberLength);
            b.Property(x => x.Lpn).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLpnLength);

            b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.LocationId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.LotBatchNumber });
            b.HasIndex(x => new { x.TenantId, x.SerialNumber });
            b.HasIndex(x => new { x.TenantId, x.Lpn });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Warehouses.WarehouseZone>()
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Warehouses.WarehouseLocation>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Products.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WarehouseManagement.Inventory.InventoryTransaction>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "InventoryTransactions", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.LotBatchNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLotBatchNumberLength);
            b.Property(x => x.SerialNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxSerialNumberLength);
            b.Property(x => x.Lpn).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLpnLength);
            b.Property(x => x.ReferenceNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxReferenceNumberLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.WarehouseId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.TransactionTime });
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber });
        });

        builder.Entity<WarehouseManagement.Inbound.InboundOrder>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "InboundOrders", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(WarehouseManagement.Inbound.InboundConsts.MaxOrderNumberLength);
            b.Property(x => x.SupplierName).IsRequired().HasMaxLength(WarehouseManagement.Inbound.InboundConsts.MaxSupplierNameLength);
            b.Property(x => x.SupplierInvoiceNumber).HasMaxLength(WarehouseManagement.Inbound.InboundConsts.MaxSupplierInvoiceNumberLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Inbound.InboundConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.WarehouseId });
            b.HasIndex(x => new { x.TenantId, x.Status });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.InboundOrderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Inbound.InboundOrderLine>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "InboundOrderLines", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ExpectedQuantity).HasPrecision(18, 4);
            b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);

            b.HasIndex(x => new { x.TenantId, x.InboundOrderId, x.ProductId });

            b.HasOne<WarehouseManagement.Products.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WarehouseManagement.Outbound.OutboundOrder>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "OutboundOrders", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxOrderNumberLength);
            b.Property(x => x.CustomerName).IsRequired().HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxCustomerNameLength);
            b.Property(x => x.ShippingAddress).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxAddressLength);
            b.Property(x => x.City).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxCityLength);
            b.Property(x => x.State).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxStateLength);
            b.Property(x => x.PostalCode).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxPostalCodeLength);
            b.Property(x => x.Country).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxCountryLength);
            b.Property(x => x.CarrierName).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxCarrierNameLength);
            b.Property(x => x.TrackingNumber).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxTrackingNumberLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Outbound.OutboundConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.WarehouseId });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Priority });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Lines)
                .WithOne()
                .HasForeignKey(x => x.OutboundOrderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Outbound.OutboundOrderLine>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "OutboundOrderLines", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
            b.Property(x => x.AllocatedQuantity).HasPrecision(18, 4);
            b.Property(x => x.PickedQuantity).HasPrecision(18, 4);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);

            b.HasIndex(x => new { x.TenantId, x.OutboundOrderId, x.ProductId });

            b.HasOne<WarehouseManagement.Products.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WarehouseManagement.Transfers.StockTransfer>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "StockTransfers", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TransferNumber).IsRequired().HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxTransferNumberLength);
            b.Property(x => x.CarrierName).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxCarrierNameLength);
            b.Property(x => x.TrackingNumber).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxTrackingNumberLength);
            b.Property(x => x.VehicleNumber).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxVehicleNumberLength);
            b.Property(x => x.DriverName).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxDriverNameLength);
            b.Property(x => x.DriverPhone).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxDriverPhoneLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Transfers.TransferConsts.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.TransferNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.SourceWarehouseId });
            b.HasIndex(x => new { x.TenantId, x.DestinationWarehouseId });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.TransferType });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.SourceWarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.DestinationWarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.StockTransferId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Transfers.StockTransferItem>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "StockTransferItems", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            b.Property(x => x.QuantityTransferred).HasPrecision(18, 4);
            b.Property(x => x.LotBatchNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLotBatchNumberLength);
            b.Property(x => x.SerialNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxSerialNumberLength);
            b.Property(x => x.Lpn).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLpnLength);

            b.HasIndex(x => new { x.StockTransferId, x.ProductId });

            b.HasOne<WarehouseManagement.Products.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WarehouseManagement.Audits.CycleCountAudit>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "CycleCountAudits", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AuditNumber).IsRequired().HasMaxLength(WarehouseManagement.Audits.AuditConsts.MaxAuditNumberLength);
            b.Property(x => x.AssignedAuditorName).HasMaxLength(WarehouseManagement.Audits.AuditConsts.MaxAuditorNameLength);
            b.Property(x => x.ApprovedByFacultyName).HasMaxLength(WarehouseManagement.Audits.AuditConsts.MaxAuditorNameLength);
            b.Property(x => x.Notes).HasMaxLength(WarehouseManagement.Audits.AuditConsts.MaxNotesLength);
            b.Property(x => x.TotalVarianceQuantity).HasPrecision(18, 4);
            b.Property(x => x.TotalVarianceValue).HasPrecision(18, 2);

            b.HasIndex(x => x.AuditNumber).IsUnique();
            b.HasIndex(x => new { x.WarehouseId, x.Status });

            b.HasOne<WarehouseManagement.Warehouses.Warehouse>()
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.CycleCountAuditId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WarehouseManagement.Audits.CycleCountAuditItem>(b =>
        {
            b.ToTable(WarehouseManagementConsts.DbTablePrefix + "CycleCountAuditItems", WarehouseManagementConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SystemQuantity).HasPrecision(18, 4);
            b.Property(x => x.CountedQuantity).HasPrecision(18, 4);
            b.Property(x => x.UnitCost).HasPrecision(18, 2);
            b.Property(x => x.LotBatchNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxLotBatchNumberLength);
            b.Property(x => x.SerialNumber).HasMaxLength(WarehouseManagement.Inventory.InventoryConsts.MaxSerialNumberLength);
            b.Property(x => x.ReasonNotes).HasMaxLength(WarehouseManagement.Audits.AuditConsts.MaxReasonNotesLength);

            b.HasIndex(x => new { x.CycleCountAuditId, x.ProductId, x.LocationId });

            b.HasOne<WarehouseManagement.Products.Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<WarehouseManagement.Warehouses.WarehouseLocation>()
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
