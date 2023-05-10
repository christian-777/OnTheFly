using MongoDB.Driver;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.Connections
{
    public class AirCraftConnection
    {
        private readonly IMongoDatabase _database;
        public AirCraftConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("AirCraft");
        }

        public AirCraft Insert(AirCraft airCraft)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
            collection.InsertOne(airCraft);
            var res = collection.Find(a => a.RAB == airCraft.RAB).FirstOrDefault();
            return res;
        }

        public List<AirCraft> FindAll()
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
            return collection.Find<AirCraft>(a => true).ToList();
        }

        public AirCraft FindByRAB(string RAB)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
            return collection.Find(a => a.RAB == RAB).FirstOrDefault();
        }

        public AirCraft Delete(string rab)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
            var collection2 = _database.GetCollection<AirCraft>("DeletedAirCraft");

            var filter = Builders<AirCraft>.Filter.Eq("RAB", rab);

            AirCraft? aircraft = collection.FindOneAndDelete(filter);
            if (aircraft != null) collection2.InsertOne(aircraft);

            return aircraft;
        }

        public AirCraft? Update(string rab, AirCraft airCraft)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
            collection.ReplaceOne(a => a.RAB == rab, airCraft);
            return collection.Find(a => a.RAB == airCraft.RAB).First();
        }

        public AirCraft? PatchDate(string rab, DateTime date)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCraft");
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