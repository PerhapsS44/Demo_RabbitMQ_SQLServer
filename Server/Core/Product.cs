using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Product
    {
        public Product(string name, int quantity, decimal price)
        {
            this.name = name;
            this.quantity = quantity;
            this.price = price;
        }
        public string name { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
}
