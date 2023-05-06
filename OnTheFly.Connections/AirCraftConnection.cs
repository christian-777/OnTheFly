using MongoDB.Driver;
using OnTheFly.Models;

namespace OnTheFly.Connections
{
    public class AirCraftConnection
    {
        public IMongoDatabase Database { get; private set; }
        public AirCraftConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("AirCraft");
        }

        public void Insert(AirCraft airCraft)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.InsertOne(airCraft);
        }

        public List<AirCraft> FindAll()
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            return collection.Find(a=>true).ToList();
        }

        public void Delete(string rab)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            var collection2 = Database.GetCollection<AirCraft>("InactiveAirCraft");

            var trash = collection.Find<AirCraft>(a=>a.RAB==rab).FirstOrDefault();

            collection2.InsertOne(trash);

            collection.FindOneAndDelete(a=> a.RAB==rab);
        }

        public void Update(string rab, AirCraft airCraft)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.ReplaceOne(a => a.RAB == rab, airCraft);
        }
    }
}