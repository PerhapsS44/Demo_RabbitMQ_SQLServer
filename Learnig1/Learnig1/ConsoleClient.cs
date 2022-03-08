using Newtonsoft.Json;
using System;
using Core;
using RabbitMQHandlerClass;
using Logger;
using LoggerTemplate;

namespace Learning1
{
    class ConsoleClient
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;
        static LoggerInterface logger;
        public static void SendMessage(Message msg)
        {
            channelClientToServer.Send(JsonConvert.SerializeObject(msg));
        }
        // constructing the message from the input string and sending it
        public static void ProcessMessage(Message.MessageTypes msgType, String[] line)
        {
            int quantity;
            decimal price;
            if (line.Length < 4 && msgType != Message.MessageTypes.ShowList)
            {
                Console.WriteLine("Invalid Operation! Too few arguments!");
                logger.LogWarning("Invalid Operation! Too few arguments!");
                return;
            }
            try
            {
                Message msg;
                if (msgType == Message.MessageTypes.ShowList)
                {
                    msg = new Message(msgType);
                }
                else
                {
                    quantity = Convert.ToInt32(line[2]);
                    price = Convert.ToDecimal(line[3]);
                    Product product = new Product(line[1], quantity, price);
                    msg = new Message(msgType, JsonConvert.SerializeObject(product));
                }
                SendMessage(msg);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Operation! Quantity is not a number");
                logger.LogWarning("Invalid Operation! Quantity is not a number");
            }
        }

        static Message? ConstructMessage(string[] inputData)
        {
            Message.MessageTypes? messageType = Message.GetMsgTypes(inputData[0]);
            Message message = null;
            Product product;
            Customer customer;
            if (messageType == null)
            {
                if (inputData[0] == "Exit")
                {
                    return new Message(Message.MessageTypes.ERR);
                }
                Console.WriteLine("ceva nu e bine");
                return null;
            }

            switch (messageType)
            {
                case Message.MessageTypes.AddProduct:
                case Message.MessageTypes.ModifyProduct:
                    if (inputData.Length != 4)
                    {
                        return null;
                    }
                    product = new Product(inputData[1], Convert.ToInt32(inputData[2]), Convert.ToDecimal(inputData[3]));
                    message = new Message((Message.MessageTypes)messageType,
                                          JsonConvert.SerializeObject(product));
                    break;
                case Message.MessageTypes.AddToBasket:
                    if (inputData.Length != 3)
                    {
                        return null;
                    }
                    product = new Product(inputData[1], Convert.ToInt32(inputData[2]), 0);
                    message = new Message((Message.MessageTypes)messageType,
                                          JsonConvert.SerializeObject(product));
                    break;
                case Message.MessageTypes.RemoveProduct:
                case Message.MessageTypes.RemoveFromBasket:
                    if (inputData.Length != 2)
                    {
                        return null;
                    }

                    message = new Message((Message.MessageTypes)messageType,
                                          inputData[1]);
                    break;
                case Message.MessageTypes.ShowList:
                case Message.MessageTypes.Logout:
                case Message.MessageTypes.Checkout:
                    message = new Message((Message.MessageTypes)messageType, "");
                    break;

                case Message.MessageTypes.Register:
                case Message.MessageTypes.Login:
                    if (inputData.Length != 3)
                    {
                        return null;
                    }
                    customer = new Customer(inputData[1], inputData[2]);
                    message = new Message((Message.MessageTypes)messageType,
                                          JsonConvert.SerializeObject(customer));
                    break;
                default: break;
            }
            return message;
        }

        static Task ShowMessage(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        static void Main()
        {
            channelClientToServer = new RabbitMQHandler("Queue1", "MyExchange1", "RabbitMQ_CTB");
            channelServerToClient = new RabbitMQHandler("Queue2", "MyExchange2", "RabbitMQ_BTC");

            logger = Logger.Logger.Instance;
            logger.InitLogger("logfile.txt");



            DelegateCallbackRecv delegateCallback = new DelegateCallbackRecv(ShowMessage);
            channelServerToClient.Receive(delegateCallback);

            String[] consoleInput;
            while (true)
            {
                consoleInput = Console.ReadLine().Split(' ');
                Message? message = ConstructMessage(consoleInput);
                if (message == null)
                {
                    Console.WriteLine("Invalid operation!");
                    continue;
                }
                if (message.type == Message.MessageTypes.ERR)
                {
                    SendMessage(message);
                    break;
                }
                SendMessage(message);
            }
        }
    }
}