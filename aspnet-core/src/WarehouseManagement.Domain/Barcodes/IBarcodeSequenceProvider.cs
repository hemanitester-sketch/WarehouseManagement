using System.Threading.Tasks;

namespace WarehouseManagement.Barcodes;

public interface IBarcodeSequenceProvider
{
    Task<long> GetNextSequenceNumberAsync(string sequenceName = "app_barcode_internal_seq");
}

