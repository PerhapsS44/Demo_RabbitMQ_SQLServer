
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
                        string toSend = await sqlConnectionHandler.ShowSomethingAsync();
                        channelServerToClient.Send(toSend);
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

            if (configData.DatabaseType == "SQL_Server"){
                sqlConnectionHandler = new DB_SQLHandler();
            } else if (configData.DatabaseType == "Text_File")
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