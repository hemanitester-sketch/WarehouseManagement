using System;
using System.Threading.Tasks;

namespace WarehouseManagement.Barcodes;

public interface IBarcodeGenerator
{
    Task<string> GenerateInternalCode128Async(BarcodeTargetType targetType, Guid? tenantId = null);

    bool ValidateGtinChecksum(string gtin);

    string CalculateGtinCheckDigit(string rawDigitsWithoutCheckDigit);

    BarcodeSymbology DetectSymbology(string barcodeValue);

    string NormalizeBarcode(string barcodeValue);
}

