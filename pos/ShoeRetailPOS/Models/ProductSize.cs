namespace ShoeRetailPOS.Models
{
    public class ProductSize
    {
        public int SizeId { get; set; }
        public int ProductId { get; set; }

        public string SizeValue { get; set; }
        public int Stock { get; set; }

        // ✅ SKU / BARCODE HERE
        public string Sku { get; set; }

        // This keeps ComboBox working even if DisplayMemberPath is forgotten
        public override string ToString()
        {
            return SizeValue;
        }
    }
}
