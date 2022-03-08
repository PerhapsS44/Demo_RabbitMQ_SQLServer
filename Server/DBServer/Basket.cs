using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Core;

namespace Server
{
    internal class Basket
    {
        List<Product> products;
        Dictionary<string, Product> productsByName;

        public Basket()
        {
            products = new List<Product>();
            productsByName = new Dictionary<string, Product>();
        }

        public void AddProduct(Product product)
        {
            Product found;
            if (productsByName.TryGetValue(product.name, out found))
            {
                found.quantity += product.quantity;
            } else
            {
                products.Add(product);
                productsByName.Add(product.name, product);
            }
        }

        public void RemoveProduct(Product product)
        {
            Product found;
            if (productsByName.TryGetValue(product.name, out found))
            {
                if (found.quantity >= product.quantity)
                {
                    found.quantity -= product.quantity;
                }
                else
                {
                    found.quantity = 0;
                }
            }
        }

        public List<Product> Products { get { return products; } }

        public void Empty()
        {
            products.Clear();
            productsByName.Clear();
        }
    }
}
