
using RabbitMQHandlerClass;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Core;
using DatabaseFileHandler;
using DatabaseSqlServerHandler;

namespace DBServer
{
    public static class Server
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;
        static DBHandlerInterface sqlConnectionHandler;
        static async Task CallDatabase(string stringMessage)
        {
            //Console.WriteLine(stringMessage);
            Message? message = JsonConvert.DeserializeObject<Message>(stringMessage);
            if (message == null) { return; }
            else
            {
                Message token;
                //Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] callToDB");
                switch (message.type)
                {
                    case Message.MessageTypes.AddProduct:
                        sqlConnectionHandler.AddSomething(message);
                        break;
                    case Message.MessageTypes.RemoveProduct:
                        sqlConnectionHandler.RemoveSomething(message);
                        break;
                    case Message.MessageTypes.ModifyProduct:
                        sqlConnectionHandler.ModifySomething(message);
                        break;
                    case Message.MessageTypes.ShowList:
                        string toSend = await sqlConnectionHandler.ShowSomethingAsync();
                        channelServerToClient.Send(toSend);
                        break;

                    case Message.MessageTypes.Register:
                        sqlConnectionHandler.Register(message);
                        break;
                    case Message.MessageTypes.Login:
                        token = await sqlConnectionHandler.LoginAsync(message);
                        if (token.type != Message.MessageTypes.ACK)
                        {
                            // invalid creditentials
                            channelServerToClient.Send("Invalid creditentials!");
                        }
                        else
                        {
                            channelServerToClient.Send("Login successful!");
                        }
                        break;
                    case Message.MessageTypes.Logout:
                        //sqlConnectionHandler.Logout();
                        break;

                    case Message.MessageTypes.AddToBasket:
                        //sqlConnectionHandler.AddToBasket(message);
                        break;
                    case Message.MessageTypes.RemoveFromBasket:
                        //sqlConnectionHandler.RemoveFromBasket(message);
                        break;
                    case Message.MessageTypes.Checkout:
                        //token = await sqlConnectionHandler.CheckoutAsync();
                        //if (token.type != Message.MessageTypes.ACK)
                        //{
                        //    // error at checkout
                        //    channelServerToClient.Send("Error!");
                        //}
                        //else
                        //{
                        //    // update the database
                        //    channelServerToClient.Send("Checkout successful!");
                        //}
                        break;
                    default:
                        break;
                }
            }
        }
        static void Main()
        {
            string configFilePath = $"{Directory.GetCurrentDirectory()}\\..\\..\\..\\config.json";
            ConfigurationParser configData = JsonConvert.DeserializeObject<ConfigurationParser>(File.ReadAllText(configFilePath));

            if (configData.DatabaseType == "SQL_Server")
            {
                sqlConnectionHandler = new DB_SQLHandler();
            }
            else if (configData.DatabaseType == "Text_File")
            {
                sqlConnectionHandler = new DB_FileHandler();
            }
            sqlConnectionHandler.DBConnect();
            channelClientToServer = new RabbitMQHandler(configData.RabbitMQClientServerQueueName,
                                                        configData.RabbitMQClientServerQueueExchange,
                                                        configData.RabbitMQClientServerQueueKey);

            channelServerToClient = new RabbitMQHandler(configData.RabbitMQServerClientQueueName,
                                                        configData.RabbitMQServerClientQueueExchange,
                                                        configData.RabbitMQServerClientQueueKey);
            while (true)
            {
                DelegateCallbackRecv callback = new DelegateCallbackRecv(CallDatabase);
                channelClientToServer.Receive(callback);
            }
            sqlConnectionHandler.DBDisconnect();
        }
    }
}