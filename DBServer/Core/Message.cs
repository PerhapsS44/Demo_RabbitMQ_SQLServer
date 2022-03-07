using Newtonsoft.Json;
namespace Core
{
    public class Message
    {
        public enum MessageTypes
        {
            ACK,
            ERR,

            AddProduct,
            RemoveProduct,
            ModifyProduct,
            ShowList,

            Register,
            Login,
            Logout,

            AddToBasket,
            RemoveFromBasket,
            Checkout
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
                    return MessageTypes.AddProduct;
                case "RemoveProduct":
                    return MessageTypes.RemoveProduct;
                case "ModifyProduct":
                    return MessageTypes.ModifyProduct;
                case "ShowList":
                    return MessageTypes.ShowList;

                case "Register":
                    return MessageTypes.Register;
                case "Login":
                    return MessageTypes.Login;
                case "Logout":
                    return MessageTypes.Logout;

                case "AddToCart":
                    return MessageTypes.AddToBasket;
                case "RemoveFromCart":
                    return MessageTypes.RemoveFromBasket;
                case "Checkout":
                    return MessageTypes.Checkout;
                default: return null;
            }
        }
    }
}