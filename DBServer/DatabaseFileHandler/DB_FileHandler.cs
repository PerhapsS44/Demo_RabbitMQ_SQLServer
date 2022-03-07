using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Core;
namespace DatabaseFileHandler
{
    public class DB_FileHandler : DBHandlerInterface
    {
        private FileStream file;
        private string fileName;
        public void DBConnect()
        {
            fileName = "database.txt";
        }

        public void DBDisconnect()
        {

        }

        private void OpenFileRead()
        {
            file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        private void OpenFileWrite()
        {
            file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
        }

        private void OpenFileAppend()
        {
            file = new FileStream(fileName, FileMode.Append);
        }

        private void CloseFile()
        {
            if (file != null)
            {
                file.Close();
            }
        }

        private void WriteFromTempToFile()
        {
            // scriu din temp inapoi in fisierul meu
            OpenFileWrite();
            StreamReader reader = new StreamReader("temp.txt");
            StreamWriter writer = new StreamWriter(file);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                writer.WriteLine(line);
            }
            reader.Close();
            writer.Close();

            CloseFile();
        }

        public void AddSomething(Message message)
        {
            OpenFileAppend();
            FileDBFormat data = new FileDBFormat(message.payload);
            string line = JsonConvert.SerializeObject(data);
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine(line);
            writer.Close();
            CloseFile();
        }

        public void ModifySomething(Message message)
        {
            OpenFileRead();
            StreamReader reader = new StreamReader(file);
            StreamWriter writer = new StreamWriter("temp.txt");
            string? line;
            line = reader.ReadLine();
            // scriu din fisierul meu intr un temp
            Product reference = JsonConvert.DeserializeObject<Product>(message.payload);
            while (line != null)
            {
                Product product = JsonConvert.DeserializeObject<Product>(line);
                if (product.name == reference.name)
                {
                    product.price = reference.price;
                    product.price = reference.quantity;
                    // trebuie sa scriu inapoi linia asta
                    line = JsonConvert.SerializeObject(product);
                }
                writer.WriteLine(line);
                line = reader.ReadLine();
            }
            reader.Close();
            writer.Close();
            CloseFile();

            WriteFromTempToFile();
        }

        public void RemoveSomething(Message message)
        {
            OpenFileRead();
            StreamReader reader = new StreamReader(file);
            StreamWriter writer = new StreamWriter("temp.txt");
            string? line;
            Product reference = JsonConvert.DeserializeObject<Product>(message.payload);

            line = reader.ReadLine();
            // scriu din fisierul meu intr un temp
            while (line != null)
            {
                Product product = JsonConvert.DeserializeObject<Product>(line);
                if (product.name != reference.name)
                {
                    writer.WriteLine(line);
                }
                line = reader.ReadLine();
            }
            reader.Close();
            writer.Close();
            CloseFile();

            WriteFromTempToFile();
        }

        public Task<string> ShowSomethingAsync()
        {
            string output = "";
            OpenFileRead();
            StreamReader reader = new StreamReader(file);
            Product data;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                data = JsonConvert.DeserializeObject<Product>(line);
                output += $"{data.name} - {data.quantity} - {data.price}\n";
            }
            CloseFile();
            return Task.FromResult(output);
        }

        public void Register(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> LoginAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public void AddToBasket(Message message)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromBasket(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> CheckoutAsync()
        {
            throw new NotImplementedException();
        }
    }

    internal class FileDBFormat
    {
        public string payload { get; set; }

        public FileDBFormat(string payload)
        {
            this.payload = payload;
        }
    }
}