﻿using System;
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
    }
}
