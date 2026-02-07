using ShoeRetailPOS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ShoeRetailPOS.Views
{
    public partial class ProductDetailsWindow : Window, INotifyPropertyChanged
    {
        // =========================
        // PUBLIC BINDINGS
        // =========================
        public Product SelectedProduct { get; }
        public List<ProductSize> Sizes { get; }

        private ProductSize _selectedProductSize;
        public ProductSize SelectedProductSize
        {
            get => _selectedProductSize;
            set
            {
                if (_selectedProductSize != value)
                {
                    _selectedProductSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedSizeTemp;
        public string SelectedSizeTemp
        {
            get => _selectedSizeTemp;
            set
            {
                if (_selectedSizeTemp != value)
                {
                    _selectedSizeTemp = value;
                    OnPropertyChanged();
                }
            }
        }


        public string SelectedSize { get; private set; }
        public int Quantity { get; private set; } = 1;

        // =========================
        // CONSTRUCTOR
        // =========================
        public ProductDetailsWindow(Product product, List<ProductSize> sizes)
        {
            InitializeComponent();

            // VERY IMPORTANT
            DataContext = this;

            SelectedProduct = product;
            Sizes = sizes;

            // Product info
            ProductName.Text = product.Name;
            ProductPrice.Text = $"Rs. {product.Price:N2}";
            ProductDescription.Text = product.Description;

            if (!string.IsNullOrEmpty(product.FirstImage))
            {
                ProductImage.Source = new BitmapImage(new Uri(product.FirstImage));
            }

            // Auto-select first available size (UX-friendly)
            if (Sizes != null && Sizes.Count > 0)
            {
                SelectedSizeTemp = Sizes[0].SizeValue;
            }
        }

        // =========================
        // ADD TO CART
        // =========================
        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedSizeTemp))
            {
                MessageBox.Show("Please select a size.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int qty) || qty < 1)
            {
                MessageBox.Show(
                    "Enter a valid quantity.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(SelectedSizeTemp))
            {
                MessageBox.Show("Please select a size.");
                return;
            }

            SelectedSize = SelectedSizeTemp;

            Quantity = qty;

            DialogResult = true;
        }

        // =========================
        // QUANTITY CONTROLS
        // =========================
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out int current))
                current = 1;

            QuantityBox.Text = (current + 1).ToString();
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out int current))
                current = 1;

            if (current > 1)
                QuantityBox.Text = (current - 1).ToString();
        }

        // =========================
        // CLOSE WINDOW
        // =========================
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
