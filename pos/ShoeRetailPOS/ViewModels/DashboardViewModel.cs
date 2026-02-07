using ShoeRetailPOS.Data;
using ShoeRetailPOS.Models;
using ShoeRetailPOS.Services;
using ShoeRetailPOS.ViewModels.Base;
using ShoeRetailPOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;




namespace ShoeRetailPOS.ViewModels
{
    public class DashboardViewModel : ViewModelBase, INotifyPropertyChanged
    {
        // =========================
        // COLLECTIONS
        // =========================
        public ObservableCollection<Product> Products { get; } = new();
        private List<Product> _allProducts = new();
        public ObservableCollection<Product> CartItems { get; } = new();

        // =========================
        // SEARCH
        // =========================
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplySearchFilter();
                }
            }
        }

        // =========================
        // COMMANDS
        // =========================
        public ICommand OpenProductDetailsCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ProceedToPaymentCommand { get; }


        // =========================
        // CONSTRUCTOR
        // =========================
        public DashboardViewModel()
        {
            OpenProductDetailsCommand = new RelayCommand(OpenProductDetails);
            IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            ProceedToPaymentCommand = new RelayCommand(_ => ProceedToPayment());


            LoadProducts();
        }

        // =========================
        // ProceedToPaymnet button
        // =========================

        private void ProceedToPayment()
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show(
                    "Cart is empty.",
                    "Payment",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var window = new PaymentWindow(CartItems, Total)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                SaveOrder(window.CustomerName);
            }
        }



        // =========================
        // Save Order
        // =========================

        private void SaveOrder(string customerName)
        {
            bool paymentSucceeded = false;

            using var conn = new MySql.Data.MySqlClient.MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var orderCmd = new MySql.Data.MySqlClient.MySqlCommand(@"
            INSERT INTO orders (user_id, customer_name, total, status)
            VALUES (1, @name, @total, 'completed');
            SELECT LAST_INSERT_ID();", conn, tx);

                orderCmd.Parameters.AddWithValue("@name", customerName);
                orderCmd.Parameters.AddWithValue("@total", Total);

                int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                foreach (var item in CartItems)
                {
                    var itemCmd = new MySql.Data.MySqlClient.MySqlCommand(@"
                INSERT INTO order_items
                (order_id, product_id, size_id, product_name, price, quantity, line_total)
                VALUES (@oid, @pid, @sizeId, @name, @price, @qty, @total)", conn, tx);

                    itemCmd.Parameters.AddWithValue("@oid", orderId);
                    itemCmd.Parameters.AddWithValue("@pid", item.ProductId);
                    itemCmd.Parameters.AddWithValue("@sizeId", item.SelectedSizeId);
                    itemCmd.Parameters.AddWithValue("@name", item.Name);
                    itemCmd.Parameters.AddWithValue("@price", item.Price);
                    itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@total", item.Price * item.Quantity);

                    itemCmd.ExecuteNonQuery();

                    var stockCmd = new MySql.Data.MySqlClient.MySqlCommand(@"
                UPDATE product_sizes
                SET stock = stock - @qty
                WHERE size_id = @sizeId", conn, tx);

                    stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@sizeId", item.SelectedSizeId);

                    if (stockCmd.ExecuteNonQuery() == 0)
                        throw new Exception("Stock update failed");
                }

                tx.Commit();
                paymentSucceeded = true;
            }
            catch (Exception ex)
            {
                // 🔒 Rollback ONLY if still valid
                if (tx.Connection != null)
                    tx.Rollback();

                MessageBox.Show(
                    "Payment failed:\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            // ✅ NON-DB WORK AFTER TRANSACTION
            if (paymentSucceeded)
            {
                ReceiptService.GenerateReceipt(CartItems, Total);

                CartItems.Clear();
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));

                MessageBox.Show(
                    "Payment completed successfully!",
                    "Payment",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }







        // =========================
        // LOAD PRODUCTS
        // =========================
        private void LoadProducts()
        {
            try
            {
                var repo = new ProductRepository();
                _allProducts = repo.GetActiveProducts();

                Products.Clear();
                foreach (var p in _allProducts)
                    Products.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to load products.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================
        // SEARCH FILTER
        // =========================
        private void ApplySearchFilter()
        {
            Products.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var p in _allProducts)
                    Products.Add(p);
                return;
            }

            var filtered = _allProducts
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var p in filtered)
                Products.Add(p);
        }

        // =========================
        // OPEN PRODUCT DETAILS
        // =========================
        private void OpenProductDetails(object parameter)
        {
            if (parameter is not Product product)
                return;

            var sizes = GetSizesForProduct(product.ProductId);

            var window = new ProductDetailsWindow(product, sizes)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                var existing = CartItems.FirstOrDefault(p =>
                    p.ProductId == product.ProductId &&
                    p.SelectedSize == window.SelectedSize);

                var repo = new ProductRepository();
                var size = sizes.First(s => s.SizeValue == window.SelectedSize);

                int availableStock = repo.GetAvailableStock(product.ProductId, size.SizeId);
                int cartQty = existing?.Quantity ?? 0;

                if (cartQty + window.Quantity > availableStock)
                {
                    MessageBox.Show(
                        $"Only {availableStock - cartQty} items available in stock",
                        "Stock Limit",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (existing != null)
                {
                    // 🔒 SIZE SAFETY CHECK
                    if (existing.SelectedSizeId != size.SizeId)
                    {
                        MessageBox.Show(
                            "Size mismatch detected. Please re-add item.",
                            "Size Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }

                    existing.Quantity += window.Quantity;
                }
                else
                {
                    var cartProduct = new Product
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Price = product.Price,
                        Color = product.Color,
                        Images = product.Images,
                        MainCategory = product.MainCategory,
                        Description = product.Description,
                        IsActive = product.IsActive,

                        SelectedSize = window.SelectedSize,
                        SelectedSizeId = size.SizeId,   // ✅ THIS FIXES THE ERROR
                        Quantity = window.Quantity
                    };


                    CartItems.Add(cartProduct);
                }

                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================
        // BARCODE / SKU SUPPORT
        // =========================
        public void TryOpenProductBySku(string sku)
        {
            var repo = new ProductRepository();
            var result = repo.GetProductBySku(sku);

            if (result == null)
                return;

            var (product, size) = result.Value;

            // Check if item already exists in cart (same product + same size)
            var existing = CartItems.FirstOrDefault(p =>
                p.ProductId == product.ProductId &&
                p.SelectedSizeId == size.SizeId);

            int availableStock = repo.GetAvailableStock(product.ProductId, size.SizeId);
            int cartQty = existing?.Quantity ?? 0;

            // Stock validation
            if (cartQty + 1 > availableStock)
            {
                SystemSounds.Hand.Play(); // ❌ ERROR BEEP

                MessageBox.Show(
                    $"Only {availableStock - cartQty} items available in stock",
                    "Stock Limit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (existing != null)
            {
                // Increase quantity by 1
                existing.Quantity += 1;
            }
            else
            {
                // Add new item directly to cart
                var cartProduct = new Product
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Color = product.Color,
                    Images = product.Images,
                    MainCategory = product.MainCategory,
                    Description = product.Description,
                    IsActive = product.IsActive,

                    SelectedSize = size.SizeValue,
                    SelectedSizeId = size.SizeId,
                    Quantity = 1
                };

                CartItems.Add(cartProduct);
            }

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));

            new SoundPlayer("Assets/Sounds/Scan.wav").Play();

            // 🔔 SUCCESS BEEP

            // Clear search box after scan
            SearchText = string.Empty;
        }


        // =========================
        // GET SIZES
        // =========================
        private List<ProductSize> GetSizesForProduct(int productId)
        {
            var sizes = new List<ProductSize>();

            using var conn = new MySql.Data.MySqlClient.MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            string query = @"
                SELECT size_id, product_id, size_value, stock, sku
                FROM product_sizes
                WHERE product_id = @pid
            ";

            using var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", productId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sizes.Add(new ProductSize
                {
                    SizeId = reader.GetInt32("size_id"),
                    ProductId = reader.GetInt32("product_id"),
                    SizeValue = reader.GetString("size_value"),
                    Stock = reader.GetInt32("stock"),
                    Sku = reader["sku"]?.ToString()
                });
            }

            return sizes;
        }

        // =========================
        // CART OPERATIONS
        // =========================
        private void IncreaseQuantity(object parameter)
        {
            if (parameter is not Product product) return;

            var repo = new ProductRepository();
            var size = GetSizesForProduct(product.ProductId)
                .First(s => s.SizeValue == product.SelectedSize);

            int availableStock = repo.GetAvailableStock(product.ProductId, size.SizeId);

            if (product.Quantity + 1 > availableStock)
            {
                MessageBox.Show(
                    $"Stock limit reached. Available: {availableStock}",
                    "Out of Stock",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            product.Quantity++;

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        private void DecreaseQuantity(object parameter)
        {
            if (parameter is not Product product) return;

            if (product.Quantity > 1)
                product.Quantity--;

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        private void RemoveFromCart(object parameter)
        {
            if (parameter is not Product product) return;

            CartItems.Remove(product);
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        // =========================
        // TOTALS
        // =========================
        public decimal Subtotal => CartItems.Sum(p => p.Price * p.Quantity);
        public decimal Total => Subtotal;

        // =========================
        // PROPERTY CHANGED
        // =========================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
