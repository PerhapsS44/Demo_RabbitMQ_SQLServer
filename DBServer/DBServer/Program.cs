
using RabbitMQHandlerClass;
using MessageClass;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace DBServer
{

    public class DBConnectionHandler
    {
        SqlConnection cnn;
        string connetionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=Demo;Trusted_Connection=True;";
        bool connected = false;
        public void DBConnect()
        {
            cnn = new SqlConnection(connetionString);
            try
            {
                cnn.Open();
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
                    cnn.Close();
                    connected = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void AddSomething(Message msg)
        {
            try
            {
                string query = "INSERT INTO DemoTable(ProductName, ProductQuantity) VALUES(@ProductName, @ProductQuantity)";

                var cmd = new SqlCommand(query, cnn);

                cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = msg.productName;
                cmd.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = msg.quantity;
                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void ModifySomething(Message msg)
        {
            try
            {
                string query = "UPDATE DemoTable SET ProductQuantity=@ProductQuantity WHERE ProductName=@ProductName";

                var cmd = new SqlCommand(query, cnn);

                cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = msg.productName;
                cmd.Parameters.Add("@ProductQuantity", SqlDbType.Int).Value = msg.quantity;
                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void RemoveSomething(Message msg)
        {
            try
            {
                string query = "DELETE FROM DemoTable WHERE ProductName=@ProductName";

                var cmd = new SqlCommand(query, cnn);

                cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.VarChar, 255)).Value = msg.productName;
                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public async Task<string> ShowSomething()
        {
            string output = "";

            try
            {
                string query = "SELECT TOP 100 * FROM DemoTable";

                var cmd = new SqlCommand(query, cnn);

                cmd.Prepare();

                var dataReader = await cmd.ExecuteReaderAsync();
                while (dataReader.Read())
                {
                    output = $"{output} {dataReader.GetValue(0)} - {dataReader.GetValue(1)} - {dataReader.GetValue(2)}\n";
                }

                dataReader.Close();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] dataFromDB");

            return output;

        }
    }
    public static class DBServerExtensions
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;
        static DBConnectionHandler handler;
        static async Task CallDatabase(string msg)
        {
            Console.WriteLine(msg);
            Message? m = JsonConvert.DeserializeObject<Message>(msg);
            if (m == null) { return; }
            else
            {
                Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] callToDB");
                switch (m.msgType)
                {
                    case Message.msgTypes.AddProduct:
                        handler.AddSomething(m);
                        break;
                    case Message.msgTypes.RemoveProduct:
                        handler.RemoveSomething(m);
                        break;
                    case Message.msgTypes.ModifyProduct:
                        handler.ModifySomething(m);
                        break;
                    case Message.msgTypes.ShowList:
                        string toSend = await handler.ShowSomething();
                        channelServerToClient.Send(toSend);
                        break;
                    default:
                        break;
                }
            }
        }
        static void Main()
        {
            handler = new DBConnectionHandler();
            handler.DBConnect();
            channelClientToServer = new RabbitMQHandler("Queue1", "MyExchange1", "RabbitMQ_CTB");
            channelServerToClient = new RabbitMQHandler("Queue2", "MyExchange2", "RabbitMQ_BTC");
            while (true)
            {
                DelegateCallbackRecv callback = new DelegateCallbackRecv(CallDatabase);
                channelClientToServer.Receive(callback);
            }
            handler.DBDisconnect();
        }
    }
}