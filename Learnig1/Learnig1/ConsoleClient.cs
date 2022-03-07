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
                Message.MessageTypes? msgType = Message.GetMsgTypes(consoleInput[0]);
                if (msgType != null)
                {
                    // message is of the known types
                    ProcessMessage((Message.MessageTypes)msgType, consoleInput);
                }
                else if (consoleInput[0].Equals("Exit"))
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid operation!");
                    logger.LogWarning("Invalid operation!");

                }
            }
        }
    }
}