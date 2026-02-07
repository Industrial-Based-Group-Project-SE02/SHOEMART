using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ShoeRetailPOS.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MainCategory { get; set; }
        public decimal Price { get; set; }
        public string Color { get; set; }
        public string Images { get; set; }
        public string IsActive { get; set; }


        // =========================
        // NOTIFYING PROPERTIES
        // =========================
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        // =========================
        // SIZE SELECTION (CART)
        // =========================
        private string _selectedSize;
        public string SelectedSize
        {
            get => _selectedSize;
            set
            {
                if (_selectedSize != value)
                {
                    _selectedSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedSizeId;
        public int SelectedSizeId
        {
            get => _selectedSizeId;
            set
            {
                if (_selectedSizeId != value)
                {
                    _selectedSizeId = value;
                    OnPropertyChanged();
                }
            }
        }



        // =========================
        // IMAGE HELPER
        // =========================
        public string FirstImage
        {
            get
            {
                if (string.IsNullOrEmpty(Images)) return null;

                try
                {
                    var images = JsonSerializer.Deserialize<string[]>(Images);
                    return images?.Length > 0 ? images[0] : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        // =========================
        // INotifyPropertyChanged
        // =========================
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
