using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace WarehouseManagement.Barcodes;

public class BarcodeGenerator : DomainService, IBarcodeGenerator
{
    private readonly IBarcodeSequenceProvider _sequenceProvider;

    public BarcodeGenerator(IBarcodeSequenceProvider sequenceProvider)
    {
        _sequenceProvider = sequenceProvider;
    }

    public async Task<string> GenerateInternalCode128Async(BarcodeTargetType targetType, Guid? tenantId = null)
    {
        var prefix = targetType switch
        {
            BarcodeTargetType.ProductVariant => "PRD",
            BarcodeTargetType.LicensePlate => "LPN",
            BarcodeTargetType.WarehouseLocation => "LOC",
            BarcodeTargetType.InventoryLot => "LOT",
            BarcodeTargetType.InventorySerial => "SER",
            _ => "BAR"
        };

        var scopeTag = tenantId.HasValue
            ? $"T{tenantId.Value.ToString("N")[..5].ToUpperInvariant()}"
            : "HST";

        var seq = await _sequenceProvider.GetNextSequenceNumberAsync();

        // Code 128 internal payload without symbol checksum.
        // Barcode rendering engine calculates Code 128 modulo 103 checksum during SVG/bitmap/ZPL generation.
        return $"{prefix}-{scopeTag}-{seq:D8}";
    }

    public string NormalizeBarcode(string barcodeValue)
    {
        if (string.IsNullOrWhiteSpace(barcodeValue))
        {
            return string.Empty;
        }

        // Strip non-printable ASCII and leading/trailing whitespace
        var cleaned = Regex.Replace(barcodeValue.Trim(), @"[^\x20-\x7E]", string.Empty);
        return cleaned;
    }

    public BarcodeSymbology DetectSymbology(string barcodeValue)
    {
        var normalized = NormalizeBarcode(barcodeValue);
        if (string.IsNullOrEmpty(normalized))
        {
            return BarcodeSymbology.Unclassified;
        }

        if (normalized.All(char.IsDigit))
        {
            if (normalized.Length == 12 && ValidateGtinChecksum(normalized))
            {
                return BarcodeSymbology.UpcA;
            }

            if (normalized.Length == 13 && ValidateGtinChecksum(normalized))
            {
                return BarcodeSymbology.Ean13;
            }

            if (normalized.Length == 14 && ValidateGtinChecksum(normalized))
            {
                return BarcodeSymbology.DataMatrix;
            }
        }

        if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains('\n') ||
            normalized.Length > 80)
        {
            return BarcodeSymbology.QrCode;
        }

        return BarcodeSymbology.Code128;
    }

    public bool ValidateGtinChecksum(string gtin)
    {
        if (string.IsNullOrWhiteSpace(gtin) || !gtin.All(char.IsDigit))
        {
            return false;
        }

        if (gtin.Length is not (8 or 12 or 13 or 14))
        {
            return false;
        }

        // GS1 Modulo 10 algorithm:
        // Starting from the rightmost digit (the check digit, at index length - 1),
        // odd positions from the right have weight 1, even positions have weight 3.
        var sum = 0;
        var isOddFromRight = true;

        for (var i = gtin.Length - 1; i >= 0; i--)
        {
            var digit = gtin[i] - '0';
            sum += digit * (isOddFromRight ? 1 : 3);
            isOddFromRight = !isOddFromRight;
        }

        return sum % 10 == 0;
    }

    public string CalculateGtinCheckDigit(string rawDigitsWithoutCheckDigit)
    {
        if (string.IsNullOrWhiteSpace(rawDigitsWithoutCheckDigit) || !rawDigitsWithoutCheckDigit.All(char.IsDigit))
        {
            throw new ArgumentException("Input must contain only decimal digits.", nameof(rawDigitsWithoutCheckDigit));
        }

        // Multipliers from right to left for payload digits: alternating 3, 1, 3, 1...
        var sum = 0;
        var multiplier = 3;

        for (var i = rawDigitsWithoutCheckDigit.Length - 1; i >= 0; i--)
        {
            var digit = rawDigitsWithoutCheckDigit[i] - '0';
            sum += digit * multiplier;
            multiplier = multiplier == 3 ? 1 : 3;
        }

        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit.ToString();
    }
}

