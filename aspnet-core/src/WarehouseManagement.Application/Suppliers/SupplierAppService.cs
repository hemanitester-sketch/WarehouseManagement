using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace WarehouseManagement.Suppliers;

public class SupplierAppService :
    CrudAppService<
        Supplier,
        SupplierDto,
        Guid,
        GetSupplierListDto,
        CreateUpdateSupplierDto,
        CreateUpdateSupplierDto>,
    ISupplierAppService
{
    public SupplierAppService(IRepository<Supplier, Guid> repository)
        : base(repository)
    {
        GetPolicyName = null;
        GetListPolicyName = null;
        CreatePolicyName = null;
        UpdatePolicyName = null;
        DeletePolicyName = null;
    }

    public override async Task<SupplierDto> CreateAsync(CreateUpdateSupplierDto input)
    {
        ValidateSupplierInput(input);

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(s => s.Code == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"A supplier with Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        var entity = await MapToEntityAsync(input);
        TryToSetTenantId(entity);
        await Repository.InsertAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public override async Task<SupplierDto> UpdateAsync(Guid id, CreateUpdateSupplierDto input)
    {
        ValidateSupplierInput(input);

        var entity = await Repository.GetAsync(id);

        var normalizedCode = input.Code.Trim().ToUpperInvariant();
        var codeExists = await Repository.AnyAsync(s => s.Id != id && s.Code == normalizedCode);
        if (codeExists)
        {
            throw new UserFriendlyException($"Another supplier with Code '{normalizedCode}' already exists. Please choose a unique code.");
        }

        entity.SetCode(input.Code);
        entity.SetName(input.Name);
        entity.ContactPerson = input.ContactPerson?.Trim();
        entity.Email = input.Email?.Trim();
        entity.PhoneNumber = input.PhoneNumber?.Trim();
        entity.TaxNumber = input.TaxNumber?.Trim();
        entity.Address = input.Address?.Trim();
        entity.City = input.City?.Trim();
        entity.State = input.State?.Trim();
        entity.PostalCode = input.PostalCode?.Trim();
        entity.PaymentTerms = input.PaymentTerms?.Trim() ?? "Net 30";
        entity.IsActive = input.IsActive;
        entity.Rating = input.Rating;
        entity.Notes = input.Notes?.Trim();

        await Repository.UpdateAsync(entity, autoSave: true);

        return await GetAsync(entity.Id);
    }

    public async Task<ListResultDto<SupplierLookupDto>> GetLookupAsync()
    {
        var suppliers = await Repository.GetListAsync(s => s.IsActive);

        var lookups = suppliers
            .OrderBy(s => s.Name)
            .Select(s => new SupplierLookupDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                City = s.City
            })
            .ToList();

        return new ListResultDto<SupplierLookupDto>(lookups);
    }

    protected override async Task<IQueryable<Supplier>> CreateFilteredQueryAsync(GetSupplierListDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                s => s.Code.Contains(input.Filter!) ||
                     s.Name.Contains(input.Filter!) ||
                     (s.ContactPerson != null && s.ContactPerson.Contains(input.Filter!)) ||
                     (s.PhoneNumber != null && s.PhoneNumber.Contains(input.Filter!)) ||
                     (s.Email != null && s.Email.Contains(input.Filter!)) ||
                     (s.City != null && s.City.Contains(input.Filter!)) ||
                     (s.TaxNumber != null && s.TaxNumber.Contains(input.Filter!)))
            .WhereIf(!string.IsNullOrWhiteSpace(input.City), s => s.City == input.City!)
            .WhereIf(!string.IsNullOrWhiteSpace(input.PaymentTerms), s => s.PaymentTerms == input.PaymentTerms!)
            .WhereIf(input.IsActive.HasValue, s => s.IsActive == input.IsActive!.Value);
    }

    protected override Task<Supplier> MapToEntityAsync(CreateUpdateSupplierDto createInput)
    {
        var entity = new Supplier(
            GuidGenerator.Create(),
            createInput.Code,
            createInput.Name,
            createInput.ContactPerson,
            createInput.Email,
            createInput.PhoneNumber,
            createInput.TaxNumber,
            createInput.Address,
            createInput.City,
            createInput.State,
            createInput.PostalCode,
            createInput.PaymentTerms,
            createInput.IsActive,
            createInput.Rating,
            createInput.Notes,
            CurrentTenant.Id
        );

        return Task.FromResult(entity);
    }

    protected override SupplierDto MapToGetOutputDto(Supplier entity)
    {
        return new SupplierDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ContactPerson = entity.ContactPerson,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            TaxNumber = entity.TaxNumber,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            PostalCode = entity.PostalCode,
            PaymentTerms = entity.PaymentTerms,
            IsActive = entity.IsActive,
            Rating = entity.Rating,
            Notes = entity.Notes,
            CreationTime = entity.CreationTime
        };
    }

    private static void ValidateSupplierInput(CreateUpdateSupplierDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
        {
            throw new UserFriendlyException("Supplier Code is required.");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new UserFriendlyException("Supplier Name is required.");
        }
    }
}

