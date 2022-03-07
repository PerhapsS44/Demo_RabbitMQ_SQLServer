using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface DBHandlerInterface
    {
        public void DBConnect();
        public void DBDisconnect();
        public void AddSomething(Message message);
        public void ModifySomething(Message message);
        public void RemoveSomething(Message message);
        public Task<string> ShowSomethingAsync();
    }
}

