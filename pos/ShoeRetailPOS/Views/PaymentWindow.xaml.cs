using ShoeRetailPOS.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ShoeRetailPOS.Views
{
    public partial class PaymentWindow : Window, INotifyPropertyChanged
    {
        // =========================
        // PUBLIC DATA
        // =========================
        public string CustomerName { get; private set; }
        public decimal Total { get; }

        // =========================
        // BALANCE
        // =========================
        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            private set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }

        // =========================
        // CONSTRUCTOR
        // =========================
        public PaymentWindow(IEnumerable<Product> cartItems, decimal total)
        {
            InitializeComponent();
            Total = total;
            DataContext = this;
        }

        // =========================
        // CASH INPUT
        // =========================
        private void CashBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (decimal.TryParse(CashBox.Text, out decimal cash))
                Balance = cash - Total;
            else
                Balance = 0;
        }

        // =========================
        // CONFIRM PAYMENT
        // =========================
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(CashBox.Text, out decimal cash))
            {
                MessageBox.Show("Enter valid cash amount");
                return;
            }

            if (cash < Total)
            {
                MessageBox.Show("Cash given is less than total");
                return;
            }

            CustomerName = NameBox.Text;
            DialogResult = true;
        }



        // =========================
        // INotifyPropertyChanged
        // =========================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
