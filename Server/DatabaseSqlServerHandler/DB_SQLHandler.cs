using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Core;
using Newtonsoft.Json;

namespace DatabaseSqlServerHandler
{
    public class DB_SQLHandler : DBHandlerInterface
    {
        SqlConnection connection;
        string connetionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Demo;Trusted_Connection=True;";
        bool connected = false;
        public void DBConnect()
        {
            connection = new SqlConnection(connetionString);
            try
            {
                connection.Open();
                connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DBDisconnect()
        {
            if (connected)
            {
                try
                {
                    connection.Close();
                    connected = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void AddSomething(Message message)
        {
            try
            {
                string query = "INSERT INTO DemoTable(ProductName, ProductQuantity, Price) VALUES(@ProductName, @ProductQuantity, @Price)";
                Product product = JsonConvert.DeserializeObject<Product>(message.payload);

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = product.name;
                sqlCommand.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = product.quantity;
                sqlCommand.Parameters.Add("@Price", SqlDbType.Int).Value = product.price;
                sqlCommand.Prepare();

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void ModifySomething(Message message)
        {
            try
            {
                string query = "UPDATE DemoTable SET ProductQuantity=@ProductQuantity, Price=@Price WHERE ProductName=@ProductName";
                Product product = JsonConvert.DeserializeObject<Product>(message.payload);

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = product.name;
                sqlCommand.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = product.quantity;
                sqlCommand.Parameters.Add("@Price", SqlDbType.Int).Value = product.price;

                sqlCommand.Prepare();

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void RemoveSomething(Message message)
        {
            try
            {
                string query = "DELETE FROM DemoTable WHERE ProductName=@ProductName";
                Product product = JsonConvert.DeserializeObject<Product>(message.payload);

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = product.name;
                sqlCommand.Prepare();

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public async Task<string> ShowSomethingAsync()
        {
            string output = "";

            try
            {
                string query = "SELECT TOP 100 * FROM DemoTable";

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Prepare();

                var dataReader = await sqlCommand.ExecuteReaderAsync();
                while (dataReader.Read())
                {
                    output = $"{output} {dataReader.GetValue(0)} - {dataReader.GetValue(1)} - {dataReader.GetValue(2)} - {dataReader.GetValue(3)}\n";
                }

                dataReader.Close();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return output;

        }

        public void Register(Message message)
        {
            try
            {
                string query = "INSERT INTO Customers(username, password) VALUES(@username, @password)";
                Customer customer = JsonConvert.DeserializeObject<Customer>(message.payload);

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 16)).Value = customer.username;
                sqlCommand.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar, 32)).Value = customer.password;
                sqlCommand.Prepare();

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Message?> LoginAsync(Message message)
        {
            Message response = null;
            try
            {
                string query = "SELECT COUNT(1) FROM Customers WHERE (username = @username AND password = @password)";
                Customer customer = JsonConvert.DeserializeObject<Customer>(message.payload);

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 16)).Value = customer.username;
                sqlCommand.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar, 32)).Value = customer.password;
                sqlCommand.Prepare();

                var dataReader = await sqlCommand.ExecuteReaderAsync();
                if (!dataReader.Read())
                {
                    // nu exista perechea username - password
                    response = new Message(Message.MessageTypes.ERR);
                }
                else
                {
                    // intorc ack cand s-a logat userul meu
                    response = new Message(Message.MessageTypes.ACK);
                }

                dataReader.Close();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return response;
        }

        public async Task<Message> CheckQuantityAsync(Product product)
        {
            SqlDataReader dataReader = null;
            try
            {
                string query = "SELECT TOP 100 * FROM DemoTable WHERE (ProductName=@ProductName)";

                var sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = product.name;

                sqlCommand.Prepare();

                dataReader = await sqlCommand.ExecuteReaderAsync();
                string output;
                if (!dataReader.Read())
                {
                    dataReader.Close();
                    return new Message(Message.MessageTypes.ERR);
                }
                Console.WriteLine($"{dataReader.GetSqlValue(0).ToString()} {dataReader.GetSqlValue(1).ToString()} {Convert.ToInt32(dataReader.GetSqlValue(2).ToString())} {Convert.ToDecimal(dataReader.GetSqlValue(3).ToString())}");
                if (product.quantity > Convert.ToInt32(dataReader.GetSqlValue(2).ToString()))
                {
                    dataReader.Close();
                    return new Message(Message.MessageTypes.ERR);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("err");
                Console.WriteLine(ex.Message);
            }
            if (dataReader != null)
                dataReader.Close();
            return new Message(Message.MessageTypes.ACK, JsonConvert.SerializeObject(product));
        }

        public Task CheckoutAsync(List<Product> products)
        {
            foreach (Product product in products)
            {
                try
                {
                    string query = "UPDATE DemoTable SET ProductQuantity=(ProductQuantity - @ProductQuantity) WHERE ProductName=@ProductName";
                    var sqlCommand = new SqlCommand(query, connection);

                    sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = product.name;
                    sqlCommand.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = product.quantity;

                    Console.WriteLine(product.quantity);

                    sqlCommand.Prepare();

                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            return Task.CompletedTask;
        }
    }
}
