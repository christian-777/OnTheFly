using MongoDB.Driver;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

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

        public AirCraft Insert(AirCraft airCraft)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.InsertOne(airCraft);

            return collection.Find(a => a.RAB == airCraft.RAB).First();
        }

        public List<AirCraft>? FindAll()
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            return collection.Find<AirCraft>(a => true).ToList();
        }

        public AirCraft? FindOne(string rab)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            return collection.Find<AirCraft>(a => a.RAB == rab).First();
        }

        public AirCraft Delete(string rab)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            var collection2 = Database.GetCollection<AirCraft>("InactiveAirCraft");

            var filter = Builders<AirCraft>.Filter.Eq("RAB", rab);

            AirCraft? aircraft = collection.FindOneAndDelete(filter);
            if (aircraft != null) collection2.InsertOne(aircraft);

            return aircraft;
        }

        public AirCraft? Update(string rab, AirCraft airCraft)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.ReplaceOne(a => a.RAB == rab, airCraft);
            return collection.Find(a => a.RAB == airCraft.RAB).First();
        }

        public AirCraft? PatchDate(string rab, DateTime date)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            AirCraft? airCraft = collection.Find(a => a.RAB == rab).FirstOrDefault();
            if (airCraft == null) return null;

            airCraft.DtLastFlight = date;

            var filter = Builders<AirCraft>.Filter.Eq("RAB", rab);
            var update = Builders<AirCraft>.Update.Set("DtLastFlight", date);
            collection.UpdateOne(filter, update);

            return airCraft;
        }
    }
}