
using RabbitMQHandlerClass;
using MessageClass;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace DBServer
{

    public class DBConnectionHandler
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

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = message.productName;
                sqlCommand.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = message.quantity;
                sqlCommand.Parameters.Add("@Price", SqlDbType.Int).Value = message.price;
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

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = message.productName;
                sqlCommand.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = message.quantity;
                sqlCommand.Parameters.Add("@Price", SqlDbType.Int).Value = message.price;

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

                var sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = message.productName;
                sqlCommand.Prepare();

                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public async Task<string> ShowSomething()
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
            //Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] dataFromDB");

            return output;

        }
    }
    public static class DBServerExtensions
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;
        static DBConnectionHandler sqlConnectionHandler;
        static async Task CallDatabase(string stringMessage)
        {
            //Console.WriteLine(stringMessage);
            Message? message = JsonConvert.DeserializeObject<Message>(stringMessage);
            if (message == null) { return; }
            else
            {
                //Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] callToDB");
                switch (message.msgType)
                {
                    case Message.MsgTypes.AddProduct:
                        sqlConnectionHandler.AddSomething(message);
                        break;
                    case Message.MsgTypes.RemoveProduct:
                        sqlConnectionHandler.RemoveSomething(message);
                        break;
                    case Message.MsgTypes.ModifyProduct:
                        sqlConnectionHandler.ModifySomething(message);
                        break;
                    case Message.MsgTypes.ShowList:
                        string toSend = await sqlConnectionHandler.ShowSomething();
                        channelServerToClient.Send(toSend);
                        break;
                    default:
                        break;
                }
            }
        }
        static void Main()
        {
            sqlConnectionHandler = new DBConnectionHandler();
            sqlConnectionHandler.DBConnect();
            channelClientToServer = new RabbitMQHandler("Queue1", "MyExchange1", "RabbitMQ_CTB");
            channelServerToClient = new RabbitMQHandler("Queue2", "MyExchange2", "RabbitMQ_BTC");
            while (true)
            {
                DelegateCallbackRecv callback = new DelegateCallbackRecv(CallDatabase);
                channelClientToServer.Receive(callback);
            }
            sqlConnectionHandler.DBDisconnect();
        }
    }
}