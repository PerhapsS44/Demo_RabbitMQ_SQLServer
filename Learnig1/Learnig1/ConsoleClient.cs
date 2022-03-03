using Newtonsoft.Json;
using System;
using MessageClass;
using RabbitMQHandlerClass;

namespace Learning1
{
    class ConsoleClient
    {
        static RabbitMQHandler channelClientToServer;
        static RabbitMQHandler channelServerToClient;

        public static void SendMessage(Message msg)
        {
            channelClientToServer.Send(JsonConvert.SerializeObject(msg));
        }
        public static void ProcessMessage(Message.msgTypes msgType, String[] line)
        {
            int quantity;
            if (line.Length < 3 && msgType != Message.msgTypes.ShowList)
            {
                Console.WriteLine("Invalid Operation! Too few arguments!");
                return;
            }
            try
            {
                Message msg;
                if (msgType == Message.msgTypes.ShowList)
                {
                    msg = new Message(msgType);
                }
                else
                {
                    quantity = Convert.ToInt32(line[2]);
                    msg = new Message(msgType, line[1], quantity);
                }
                SendMessage(msg);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid Operation! Quantity is not a number");
            }
        }

        static Task ShowMessage(string msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        static void DoNothing(string msg)
        {

        }
        static void Main()
        {
            string testString = "string";
            channelClientToServer = new RabbitMQHandler("Queue1", "MyExchange1", "RabbitMQ_CTB");
            channelServerToClient = new RabbitMQHandler("Queue2", "MyExchange2", "RabbitMQ_BTC");

            DelegateCallbackRecv callback = new DelegateCallbackRecv(ShowMessage);
            channelServerToClient.Receive(callback);

            String[] consoleInput;
            while (true)
            {
                consoleInput = Console.ReadLine().Split(' ');
                Message.msgTypes? msgType = Message.GetMsgTypes(consoleInput[0]);
                if (msgType != null)
                {
                    // message is of the known types
                    ProcessMessage((Message.msgTypes)msgType, consoleInput);
                }
                else if (consoleInput[0].Equals("Exit"))
                {
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid operation!");
                }
            }
        }
    }
}