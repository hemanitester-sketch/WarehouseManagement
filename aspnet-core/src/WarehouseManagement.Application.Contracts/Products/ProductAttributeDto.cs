using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace WarehouseManagement.Products;

public class ProductAttributeDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public bool IsVariantDefining { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public List<ProductAttributeValueDto> Values { get; set; } = new();
}

public class ProductAttributeValueDto : EntityDto<Guid>
{
    public Guid AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public string? DisplayLabel { get; set; }

    public string? ColorHex { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}

public class CreateUpdateProductAttributeDto
{
    [Required]
    [StringLength(ProductAttributeConsts.MaxNameLength)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(ProductAttributeConsts.MaxCodeLength)]
    public string Code { get; set; } = null!;

    public AttributeDataType DataType { get; set; } = AttributeDataType.Dropdown;

    public bool IsVariantDefining { get; set; } = true;

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public List<CreateAttributeValueDto> Values { get; set; } = new();
}

public class CreateAttributeValueDto
{
    [Required]
    [StringLength(ProductAttributeConsts.MaxValueLength)]
    public string Value { get; set; } = null!;

    [StringLength(ProductAttributeConsts.MaxDisplayLabelLength)]
    public string? DisplayLabel { get; set; }

    [StringLength(ProductAttributeConsts.MaxColorHexLength)]
    public string? ColorHex { get; set; }

    public int DisplayOrder { get; set; } = 0;
}

public class CategoryAttributeTemplateDto : EntityDto<Guid>
{
    public Guid CategoryId { get; set; }

    public Guid AttributeId { get; set; }

    public string AttributeName { get; set; } = null!;

    public string AttributeCode { get; set; } = null!;

    public AttributeDataType DataType { get; set; }

    public bool IsVariantDefining { get; set; }

    public bool IsRequired { get; set; }

    public int DisplayOrder { get; set; }

    public List<ProductAttributeValueDto> PredefinedValues { get; set; } = new();
}

public class AssignCategoryAttributeDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid AttributeId { get; set; }

    public bool IsVariantDefining { get; set; } = true;

    public bool IsRequired { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;
}

