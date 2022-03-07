using Newtonsoft.Json;
namespace Core
{
    public class Message
    {
        public enum MsgTypes
        {
            AddProduct,
            RemoveProduct,
            ModifyProduct,
            ShowList
        }
        public MsgTypes msgType { get; set; }
        public string productName { get; set; }
        public int quantity { get; set; }
        public float price { get; set; }

        public Message(MsgTypes msgType)
        {
            this.msgType = msgType;
            this.productName = "";
            this.quantity = 0;
            this.price = 0;
        }
        [JsonConstructor]
        public Message(Message.MsgTypes msgType, string productName, int quantity, float price)
        {
            this.msgType = msgType;
            this.productName = productName;
            this.quantity = quantity;
            this.price = price;
        }

        public static Message.MsgTypes? GetMsgTypes(string str)
        {
            switch (str)
            {
                case "AddProduct":
                    return Message.MsgTypes.AddProduct;
                case "RemoveProduct":
                    return Message.MsgTypes.RemoveProduct;
                case "ModifyProduct":
                    return Message.MsgTypes.ModifyProduct;
                case "ShowList":
                    return Message.MsgTypes.ShowList;
                default: return null;
            }
        }
    }
}