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
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            collection.InsertOne(airCraft);
            var res = collection.Find(a => a.RAB == airCraft.RAB).FirstOrDefault();
            return res;
        }

        public List<AirCraft> FindAll()
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            return collection.Find<AirCraft>(a => true).ToList();
        }

        public AirCraft FindByRAB(string RAB)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            var t= collection.Find(a => a.RAB == RAB).FirstOrDefault();
            return collection.Find(a => a.RAB == RAB).FirstOrDefault();
        }

        public AirCraft FindByRABDeleted(string RAB)
        {
            var collectionDeleted = _database.GetCollection<AirCraft>("DeletedAirCrafts");
            return collectionDeleted.Find(a => a.RAB == RAB).FirstOrDefault();
        }

        public bool Delete(string rab)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            var collectionDeleted = _database.GetCollection<AirCraft>("DeletedAirCrafts");

            var filter = Builders<AirCraft>.Filter.Eq("RAB", rab);

            AirCraft? trash = collection.FindOneAndDelete(filter);
            if (trash == null) return false;

            collectionDeleted.InsertOne(trash);
            return true;
        }

        public bool UndeleteAirCraft(string rab)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            var collectionDeleted = _database.GetCollection<AirCraft>("DeletedAirCrafts");

            var filter = Builders<AirCraft>.Filter.Eq("RAB", rab);

            AirCraft? trash = collectionDeleted.FindOneAndDelete(filter);
            if (trash == null) return false;

            collection.InsertOne(trash);
            return true;
        }

        public bool Update(string rab, AirCraft airCraft)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
            return collection.ReplaceOne(a => a.RAB == rab, airCraft).IsAcknowledged;
        }

        public AirCraft? PatchDate(string rab, DateTime date)
        {
            var collection = _database.GetCollection<AirCraft>("ActivatedAirCrafts");
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