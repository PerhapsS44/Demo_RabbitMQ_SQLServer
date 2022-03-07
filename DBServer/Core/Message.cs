using Newtonsoft.Json;
namespace Core
{
    public class Message
    {
        public enum MessageTypes
        {
            AddProduct,
            RemoveProduct,
            ModifyProduct,
            ShowList
        }
        public MessageTypes type { get; set; }

        public string payload;

        public Message(MessageTypes msgType)
        {
            this.type = msgType;
            this.payload = "";
        }
        [JsonConstructor]
        public Message(Message.MessageTypes msgType, string payload)
        {
            this.type = msgType;
            this.payload = payload;
        }

        public static Message.MessageTypes? GetMsgTypes(string str)
        {
            switch (str)
            {
                case "AddProduct":
                    return Message.MessageTypes.AddProduct;
                case "RemoveProduct":
                    return Message.MessageTypes.RemoveProduct;
                case "ModifyProduct":
                    return Message.MessageTypes.ModifyProduct;
                case "ShowList":
                    return Message.MessageTypes.ShowList;
                default: return null;
            }
        }
    }
}