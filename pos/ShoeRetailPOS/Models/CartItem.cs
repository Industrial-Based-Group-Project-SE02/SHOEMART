using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeRetailPOS.Models
{
    internal class CartItem
    {
        public int ProductId { get; set; }
        public int SizeId { get; set; }

        public string ProductName { get; set; }
        public string Size { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
