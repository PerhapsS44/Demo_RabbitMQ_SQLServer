using MessageClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DBServer
{
    class DB_FileHandler : DBHandlerInterface
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
            FileDBFormat data = new FileDBFormat(message.productName, message.quantity, message.price);
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
            FileDBFormat data;
            line = reader.ReadLine();
            // scriu din fisierul meu intr un temp
            while (line != null)
            {
                data = JsonConvert.DeserializeObject<FileDBFormat>(line);
                if (data.productName == message.productName)
                {
                    data.productPrice = message.price;
                    data.productQuantity = message.quantity;
                    // trebuie sa scriu inapoi linia asta
                    line = JsonConvert.SerializeObject(data);
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
            FileDBFormat data;
            line = reader.ReadLine();
            // scriu din fisierul meu intr un temp
            while (line != null)
            {
                data = JsonConvert.DeserializeObject<FileDBFormat>(line);
                if (data.productName != message.productName)
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
            FileDBFormat data;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                data = JsonConvert.DeserializeObject<FileDBFormat>(line);
                output += $"{data.productName} - {data.productQuantity} - {data.productPrice}\n";
            }
            CloseFile();
            return Task.FromResult(output);
        }
    }

    internal class FileDBFormat
    {
        public string productName { get; set; }
        public int productQuantity { get; set; }
        public float productPrice { get; set; }

        public FileDBFormat(string productName, int productQuantity, float productPrice)
        {
            this.productName = productName;
            this.productQuantity = productQuantity;
            this.productPrice = productPrice;
        }
    }
}
