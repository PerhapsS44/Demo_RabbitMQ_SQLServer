using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Customer
    {
        public string username { get; set; }
        public string password { get; set; }

        public Customer(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}
