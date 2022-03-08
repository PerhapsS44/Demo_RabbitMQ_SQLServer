using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Core
{
    public class ConfigurationParser
    {
        public ConfigurationParser()
        {
        }
        [JsonProperty("databaseType")]
        public string DatabaseType { get; set; }
        [JsonProperty("databaseArgs")]
        public string DatabaseArgs { get; set; }

        [JsonProperty("rabbitMQClientServerQueueName")]
        public string RabbitMQClientServerQueueName { get; set; }
        [JsonProperty("rabbitMQClientServerQueueExchange")]
        public string RabbitMQClientServerQueueExchange { get; set; }
        [JsonProperty("rabbitMQClientServerQueueKey")]
        public string RabbitMQClientServerQueueKey { get; set; }

        [JsonProperty("rabbitMQServerClientQueueName")]
        public string RabbitMQServerClientQueueName { get; set; }
        [JsonProperty("rabbitMQServerClientQueueExchange")]
        public string RabbitMQServerClientQueueExchange { get; set; }
        [JsonProperty("rabbitMQServerClientQueueKey")]
        public string RabbitMQServerClientQueueKey { get; set; }



    }
}
