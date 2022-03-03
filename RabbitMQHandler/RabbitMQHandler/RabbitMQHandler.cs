using RabbitMQ.Client;
using System.Text;
using System;
using System.Globalization;

namespace RabbitMQHandlerClass
{
    public delegate Task DelegateCallbackRecv(string msg);

    public class MessageReceiver : AsyncDefaultBasicConsumer
    {
        private readonly IModel _channel;
        private DelegateCallbackRecv o = null;
        public MessageReceiver(IModel channel, DelegateCallbackRecv o)
        {
            _channel = channel;
            this.o = o;
        }
        public override Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {


            //Console.WriteLine(string.Concat("Message: ", Encoding.UTF8.GetString(body.ToArray())));

            // use the message
            string msg = Encoding.UTF8.GetString(body.ToArray());
            Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] {msg}");
            o(msg);
            _channel.BasicAck(deliveryTag, false);
            return Task.CompletedTask;
        }

    }
    public class RabbitMQHandler
    {
        public string queueName { get; set; }
        public string routingKey { get; set; }
        public string exchangeName { get; set; }

        IModel model;
        IBasicProperties basicProperties;
        public RabbitMQHandler(string queueName, string routingKey, string exchangeName)
        {
            this.queueName = queueName;
            this.routingKey = routingKey;
            this.exchangeName = exchangeName;
            var connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = "localhost";
            connectionFactory.UserName = "demo_app";
            connectionFactory.Password = "greatpass!";
            connectionFactory.DispatchConsumersAsync = true;

            IConnection connection =
                        connectionFactory.CreateConnection();
            model = connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct, true); //"MyExchange"

            model.QueueDeclare(queue: queueName,
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
            model.QueueBind(queueName, exchangeName, routingKey, //RabbitMQ_CTB
new Dictionary<string, object>());
            basicProperties = model.CreateBasicProperties();
            model.BasicQos(0, 1, false);
        }
        public void Send(string message)
        {
            Console.WriteLine($"[debug] timestamp: [{DateTime.Now.ToFileTimeUtc()}] {message}");
            model.BasicPublish(exchangeName, routingKey, false,
                basicProperties, Encoding.UTF8.GetBytes(message));
        }

        public void Receive(DelegateCallbackRecv o)
        {
            MessageReceiver messageReceiver = new MessageReceiver(model, o);
            model.BasicConsume(queueName, false, messageReceiver);


        }
    }
}