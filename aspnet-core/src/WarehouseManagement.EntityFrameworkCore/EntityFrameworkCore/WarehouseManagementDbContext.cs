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
    }
}
