using ShoeRetailPOS.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShoeRetailPOS.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        // 🔥 THIS IS THE MISSING PIECE
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (DataContext is not DashboardViewModel vm)
                return;

            var input = vm.SearchText?.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            // SKU / BARCODE PATH
            // (SKUs usually contain dash or are long)
            if (input.Contains("-") || input.Length > 6)
            {
                vm.TryOpenProductBySku(input);
                e.Handled = true;
            }
        }
    }
}
