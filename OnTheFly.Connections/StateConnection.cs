using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using OnTheFly.Models;

namespace OnTheFly.Connections
{
    public class StateConnection
    {
        public IMongoDatabase Database { get; private set; }
        public StateConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("Airport");
        }

        public State? GetUF(string name)
        {
            
            var collection = Database.GetCollection<State>("States");
            State? state = collection.Find(s => s.Name == name).FirstOrDefault();
            return state;
        }
    }
}
