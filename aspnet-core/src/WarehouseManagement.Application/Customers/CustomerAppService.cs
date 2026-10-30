using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace WarehouseManagement.Customers;

public class CustomerAppService :
    CrudAppService<
        Customer,
        CustomerDto,
        Guid,
        GetCustomerListDto,
        CreateUpdateCustomerDto,
        CreateUpdateCustomerDto>,
    ICustomerAppService
{
    public CustomerAppService(IRepository<Customer, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public override async Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListDto input)
    {
        await EnsureSeedDataAsync();
        return await base.GetListAsync(input);
    }

    public override async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        ValidateCustomerInput(input);

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(c => c.Code == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"A customer with Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        var entity = await MapToEntityAsync(input);
        TryToSetTenantId(entity);
        await Repository.InsertAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public override async Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {
        ValidateCustomerInput(input);

        var entity = await Repository.GetAsync(id);

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(c => c.Id != id && c.Code == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"Another customer with Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        entity.SetCode(input.Code);
        entity.SetName(input.Name);
        entity.CustomerType = input.CustomerType;
        entity.Industry = input.Industry?.Trim();
        entity.ContactPerson = input.ContactPerson?.Trim();
        entity.Email = input.Email?.Trim();
        entity.PhoneNumber = input.PhoneNumber?.Trim();
        entity.AlternativePhone = input.AlternativePhone?.Trim();
        entity.TaxNumber = input.TaxNumber?.Trim();
        entity.PanNumber = input.PanNumber?.Trim();
        entity.BillingAddress = input.BillingAddress?.Trim();
        entity.ShippingAddress = input.ShippingAddress?.Trim();
        entity.City = input.City?.Trim();
        entity.State = input.State?.Trim();
        entity.PostalCode = input.PostalCode?.Trim();
        entity.Country = input.Country?.Trim() ?? "India";
        entity.PaymentTerms = input.PaymentTerms?.Trim() ?? "Net 30";
        entity.CreditLimit = input.CreditLimit >= 0 ? input.CreditLimit : 0m;
        entity.CurrentOutstanding = input.CurrentOutstanding >= 0 ? input.CurrentOutstanding : 0m;
        entity.PriceTier = input.PriceTier?.Trim() ?? "Tier 2 (Standard B2B)";
        entity.DeliveryDockHours = input.DeliveryDockHours?.Trim();
        entity.SpecialInstructions = input.SpecialInstructions?.Trim();
        entity.Status = input.Status;
        entity.IsActive = input.IsActive;
        entity.Rating = input.Rating;
        entity.Notes = input.Notes?.Trim();

        await Repository.UpdateAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public async Task<ListResultDto<CustomerLookupDto>> GetLookupAsync()
    {
        await EnsureSeedDataAsync();
        var customers = await Repository.GetListAsync(c => c.IsActive && c.Status != CustomerStatus.Inactive);

        var lookups = customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerLookupDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CustomerType = c.CustomerType,
                ContactPerson = c.ContactPerson,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email,
                ShippingAddress = c.ShippingAddress,
                City = c.City,
                State = c.State,
                PostalCode = c.PostalCode,
                PaymentTerms = c.PaymentTerms,
                CreditLimit = c.CreditLimit,
                CurrentOutstanding = c.CurrentOutstanding
            })
            .ToList();

        return new ListResultDto<CustomerLookupDto>(lookups);
    }

    public async Task<CustomerDashboardMetricsDto> GetDashboardMetricsAsync()
    {
        await EnsureSeedDataAsync();
        var all = await Repository.GetListAsync();

        var totalCount = all.Count;
        var activeCount = all.Count(c => c.IsActive && c.Status == CustomerStatus.Active);
        var corporateB2BCount = all.Count(c => c.CustomerType == CustomerType.CorporateB2B);
        var retailDistributorCount = all.Count(c => c.CustomerType == CustomerType.RetailB2C || c.CustomerType == CustomerType.Distributor);
        var totalCreditLimit = all.Sum(c => c.CreditLimit);
        var totalOutstanding = all.Sum(c => c.CurrentOutstanding);
        var onHoldCount = all.Count(c => c.Status == CustomerStatus.OnHold);

        var avgUtilization = totalCreditLimit > 0
            ? Math.Min(100m, Math.Round((totalOutstanding / totalCreditLimit) * 100m, 1))
            : 0m;

        return new CustomerDashboardMetricsDto
        {
            TotalClientsCount = totalCount,
            ActiveClientsCount = activeCount,
            CorporateB2BCount = corporateB2BCount,
            RetailDistributorCount = retailDistributorCount,
            TotalCreditLimitExposure = totalCreditLimit,
            TotalCurrentOutstanding = totalOutstanding,
            AverageCreditUtilizationPercent = avgUtilization,
            OnHoldClientsCount = onHoldCount
        };
    }

    public async Task<CustomerDto> ToggleStatusAsync(Guid id, CustomerStatus newStatus)
    {
        var entity = await Repository.GetAsync(id);
        entity.Status = newStatus;
        entity.IsActive = (newStatus == CustomerStatus.Active);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    protected override async Task<IQueryable<Customer>> CreateFilteredQueryAsync(GetCustomerListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                c => c.Code.Contains(input.Filter!) ||
                     c.Name.Contains(input.Filter!) ||
                     (c.ContactPerson != null && c.ContactPerson.Contains(input.Filter!)) ||
                     (c.PhoneNumber != null && c.PhoneNumber.Contains(input.Filter!)) ||
                     (c.Email != null && c.Email.Contains(input.Filter!)) ||
                     (c.City != null && c.City.Contains(input.Filter!)) ||
                     (c.TaxNumber != null && c.TaxNumber.Contains(input.Filter!)) ||
                     (c.Industry != null && c.Industry.Contains(input.Filter!)))
            .WhereIf(input.CustomerType.HasValue, c => c.CustomerType == input.CustomerType!.Value)
            .WhereIf(input.Status.HasValue, c => c.Status == input.Status!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.City), c => c.City == input.City!)
            .WhereIf(!string.IsNullOrWhiteSpace(input.PaymentTerms), c => c.PaymentTerms == input.PaymentTerms!)
            .WhereIf(input.IsActive.HasValue, c => c.IsActive == input.IsActive!.Value);
    }

    protected override Task<Customer> MapToEntityAsync(CreateUpdateCustomerDto createInput)
    {
        var entity = new Customer(
            GuidGenerator.Create(),
            createInput.Code,
            createInput.Name,
            createInput.CustomerType,
            createInput.Industry,
            createInput.ContactPerson,
            createInput.Email,
            createInput.PhoneNumber,
            createInput.AlternativePhone,
            createInput.TaxNumber,
            createInput.PanNumber,
            createInput.BillingAddress,
            createInput.ShippingAddress,
            createInput.City,
            createInput.State,
            createInput.PostalCode,
            createInput.Country,
            createInput.PaymentTerms,
            createInput.CreditLimit,
            createInput.CurrentOutstanding,
            createInput.PriceTier,
            createInput.DeliveryDockHours,
            createInput.SpecialInstructions,
            createInput.Status,
            createInput.IsActive,
            createInput.Rating,
            0,
            0m,
            createInput.Notes,
            CurrentTenant.Id
        );

        return Task.FromResult(entity);
    }

    protected override CustomerDto MapToGetOutputDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            CustomerType = entity.CustomerType,
            Industry = entity.Industry,
            ContactPerson = entity.ContactPerson,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            AlternativePhone = entity.AlternativePhone,
            TaxNumber = entity.TaxNumber,
            PanNumber = entity.PanNumber,
            BillingAddress = entity.BillingAddress,
            ShippingAddress = entity.ShippingAddress,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            Country = entity.Country,
            PaymentTerms = entity.PaymentTerms,
            CreditLimit = entity.CreditLimit,
            CurrentOutstanding = entity.CurrentOutstanding,
            PriceTier = entity.PriceTier,
            DeliveryDockHours = entity.DeliveryDockHours,
            SpecialInstructions = entity.SpecialInstructions,
            Status = entity.Status,
            IsActive = entity.IsActive,
            Rating = entity.Rating,
            TotalOrdersPlaced = entity.TotalOrdersPlaced,
            TotalRevenue = entity.TotalRevenue,
            Notes = entity.Notes,
            CreationTime = entity.CreationTime
        };
    }

    private static void ValidateCustomerInput(CreateUpdateCustomerDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
        {
            throw new UserFriendlyException("Customer Code is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserFriendlyException("Customer Name is required.");
        }
    }

    private async Task EnsureSeedDataAsync()
    {
        if (await Repository.AnyAsync())
        {
            return;
        }

        var seeds = new List<Customer>
        {
            new Customer(
                GuidGenerator.Create(),
                "CUST-1001",
                "Reliance Retail Logistics Ltd",
                CustomerType.CorporateB2B,
                "Retail & Consumer Goods",
                "Rajesh Verma",
                "r.verma@relianceretail.com",
                "+91 98200 11223",
                "+91 22 4556 7890",
                "27AAACR1234F1Z8",
                "AAACR1234F",
                "Reliance Corporate Park, Thane-Belapur Road",
                "National Logistics Hub, Gate 4, Bhiwandi",
                "Mumbai",
                "Maharashtra",
                "400701",
                "India",
                "Net 45",
                2500000m,
                640000m,
                "Tier 1 (Wholesale VIP)",
                "08:00 AM - 08:00 PM (Mon-Sat)",
                "Palletized deliveries required. Unloading bay #12. Gate pass needed.",
                CustomerStatus.Active,
                true,
                5,
                48,
                14250000m,
                "Key enterprise account with pan-India distribution.",
                CurrentTenant.Id
            ),
            new Customer(
                GuidGenerator.Create(),
                "CUST-1002",
                "Tata Electronics B2B Systems",
                CustomerType.CorporateB2B,
                "Consumer Electronics",
                "Sneha Deshmukh",
                "sneha.d@tataelectronics.in",
                "+91 98450 33445",
                "+91 80 6778 9900",
                "29AABCT5678Q1Z2",
                "AABCT5678Q",
                "Whitefield Tech Zone, Plot 14",
                "Hub 3, Bangalore Central Fulfillment Center",
                "Bengaluru",
                "Karnataka",
                "560066",
                "India",
                "Net 30",
                1500000m,
                380000m,
                "Tier 1 (Wholesale VIP)",
                "09:00 AM - 06:00 PM",
                "ESD safe handling required for micro-components.",
                CustomerStatus.Active,
                true,
                5,
                32,
                8900000m,
                "Precision component deliveries.",
                CurrentTenant.Id
            ),
            new Customer(
                GuidGenerator.Create(),
                "CUST-1003",
                "Flipkart India Pvt Ltd (B2B Hub)",
                CustomerType.EcommercePartner,
                "E-Commerce & Quick Commerce",
                "Amitabh Khurana",
                "amitabh.k@flipkart.com",
                "+91 99100 55667",
                "+91 124 499 8800",
                "06AAACB8910R1Z4",
                "AAACB8910R",
                "DLF Cyber City, Tower B, Phase 2",
                "Tauru Warehouse Complex, Dock 8",
                "Gurugram",
                "Haryana",
                "122002",
                "India",
                "Net 30",
                3000000m,
                1250000m,
                "Tier 1 (Wholesale VIP)",
                "24x7 Inwarding Window",
                "Barcoded outer carton scan mandatory prior to dock acceptance.",
                CustomerStatus.Active,
                true,
                4,
                75,
                24500000m,
                "High-volume daily replenishment client.",
                CurrentTenant.Id
            ),
            new Customer(
                GuidGenerator.Create(),
                "CUST-1004",
                "Apollo Healthcare Logistics",
                CustomerType.CorporateB2B,
                "Pharma & Healthcare",
                "Dr. Meera Nambiar",
                "meera.n@apollohospitals.com",
                "+91 98410 77889",
                null,
                "33AAACA3456K1Z9",
                "AAACA3456K",
                "Greams Road, Thousand Lights",
                "Central Cold Chain Warehouse, Guindy",
                "Chennai",
                "Tamil Nadu",
                "600006",
                "India",
                "Net 15",
                800000m,
                120000m,
                "Tier 2 (Standard B2B)",
                "08:00 AM - 04:00 PM",
                "Temperature sensitive. Maintain cold chain at 2-8°C during transit.",
                CustomerStatus.Active,
                true,
                5,
                19,
                4200000m,
                "Pharma supplies and certified equipment.",
                CurrentTenant.Id
            ),
            new Customer(
                GuidGenerator.Create(),
                "CUST-1005",
                "Vijay Sales Distribution Hub",
                CustomerType.Distributor,
                "Retail Electronics",
                "Nilesh Shah",
                "nilesh.shah@vijaysales.com",
                "+91 97240 88990",
                null,
                "24AAACV7890M1Z3",
                "AAACV7890M",
                "Drive In Road, Navrangpura",
                "Narol Industrial Estate, Unit 12",
                "Ahmedabad",
                "Gujarat",
                "380009",
                "India",
                "Net 30",
                1200000m,
                450000m,
                "Tier 2 (Standard B2B)",
                "10:00 AM - 07:00 PM",
                "Check serial numbers against manifest before dispatch acknowledgment.",
                CustomerStatus.Active,
                true,
                4,
                27,
                6800000m,
                "Regional consumer appliances distributor.",
                CurrentTenant.Id
            ),
            new Customer(
                GuidGenerator.Create(),
                "CUST-1006",
                "Apex Industrial Fasteners & Tools",
                CustomerType.RetailB2C,
                "Industrial Machinery",
                "Vikram Chauhan",
                "vikram@apextoolcorp.com",
                "+91 94140 22334",
                null,
                "08AAACA9876P1Z1",
                "AAACA9876P",
                "MIA Industrial Area, Phase 1",
                "MIA Sector B, Depot 5",
                "Alwar",
                "Rajasthan",
                "301030",
                "India",
                "Advance",
                300000m,
                295000m,
                "Tier 3 (Retail Partner)",
                "09:00 AM - 05:00 PM",
                "Credit limit near maximum. Require advance payment for new orders.",
                CustomerStatus.OnHold,
                true,
                3,
                11,
                1200000m,
                "Account placed on temporary hold pending invoice clearance.",
                CurrentTenant.Id
            )
        };

        foreach (var seed in seeds)
        {
            await Repository.InsertAsync(seed);
        }
    }
}

