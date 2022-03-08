using RabbitMQHandlerClass;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using Core;
using DatabaseFileHandler;
using DatabaseSqlServerHandler;

namespace Server
{
    public static class Server
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;
        static DBHandlerInterface sqlConnectionHandler;
        static Basket basket;
        static bool loggedIn = false;
        static async Task CallDatabase(string stringMessage)
        {
            //Console.WriteLine(stringMessage);
            Message? message = JsonConvert.DeserializeObject<Message>(stringMessage);
            Product product = null;
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
                            loggedIn = true;
                        }
                        break;
                    case Message.MessageTypes.Logout:
                        loggedIn = false;
                        break;

                    case Message.MessageTypes.AddToBasket:
                        if (loggedIn)
                        {
                            product = JsonConvert.DeserializeObject<Product>(message.payload);
                            basket.AddProduct(product);
                        }
                        else
                        {
                            channelServerToClient.Send("You must be logged in to make purchases!");
                        }
                        break;
                    case Message.MessageTypes.RemoveFromBasket:
                        if (loggedIn)
                        {
                            product = JsonConvert.DeserializeObject<Product>(message.payload);
                            basket.RemoveProduct(product);
                        }
                        else
                        {
                            channelServerToClient.Send("You must be logged in to make purchases!");
                        }
                        break;
                    case Message.MessageTypes.Checkout:
                        if (!loggedIn)
                        {
                            channelServerToClient.Send("You must be logged in to make purchases!");
                            break;
                        }
                        List<Product> products = basket.Products;
                        bool foundError = false;
                        foreach (Product pr in products)
                        {
                            Message m = await sqlConnectionHandler.CheckQuantityAsync(pr);
                        
                            if (m.type == Message.MessageTypes.ERR)
                            {
                                // am avut o eroare undeva, am mai multe produse in cos decat in magazin
                                // intorc mesaj catre client
                                channelServerToClient.Send($"Checkout failed! Too many items of some category!");


                                foundError = true;
                                break;
                            }
                        }
                        if (!foundError)
                        {
                            // anunt clientul ca operatia a reusit
                            channelServerToClient.Send("Checkout successful!");

                            // golesc cosul si modific info din baza de date
                            sqlConnectionHandler.CheckoutAsync(products);
                            basket.Empty();

                        }
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

            basket = new Basket();

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