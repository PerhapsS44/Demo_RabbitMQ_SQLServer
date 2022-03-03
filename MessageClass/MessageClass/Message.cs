using Newtonsoft.Json;
namespace MessageClass
{
    public class Message
    {
        public enum msgTypes
        {
            AddProduct,
            RemoveProduct,
            ModifyProduct,
            ShowList
        }
        public msgTypes msgType { get; set; }
        public string productName { get; set; }
        public int quantity { get; set; }

        public Message(msgTypes msgType)
        {
            this.msgType = msgType;
            this.productName = "";
            this.quantity = 0;
        }
        [JsonConstructor]
        public Message(Message.msgTypes msgType, string productName, int quantity)
        {
            this.msgType = msgType;
            this.productName = productName;
            this.quantity = quantity;
        }

        public static Message.msgTypes? GetMsgTypes(string str)
        {
            switch (str)
            {
                case "AddProduct":
                    return Message.msgTypes.AddProduct;
                case "RemoveProduct":
                    return Message.msgTypes.RemoveProduct;
                case "ModifyProduct":
                    return Message.msgTypes.ModifyProduct;
                case "ShowList":
                    return Message.msgTypes.ShowList;
                default: return null;
            }
        }
    }
}