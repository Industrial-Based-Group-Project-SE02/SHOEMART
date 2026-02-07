using System.Windows;
using ShoeRetailPOS.Views;

namespace ShoeRetailPOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Load dashboard by default
            MainContent.Content = new DashboardView();
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView();
        }

        private void Holds_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HoldsView();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SettingsView();
        }
    }
}
