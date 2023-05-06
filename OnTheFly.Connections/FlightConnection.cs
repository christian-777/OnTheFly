using MongoDB.Driver;
using OnTheFly.Models;

namespace OnTheFly.Connections
{
    public class FlightConnection
    {
        public IMongoDatabase Database { get; private set; }
        public FlightConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("Flight");
        }

        public void Insert(Flight flight)
        {
            var activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            activeCollection.InsertOne(flight);
        }

        public List<Flight> FindAll()
        {
            var activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            return activeCollection.Find(a=>true).ToList();
        }

        public void Delete(string id)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            IMongoCollection<Flight> inactiveCollection = Database.GetCollection<Flight>("InactiveFlight");

            Flight? trash = activeCollection.FindOneAndDelete(f => f.Id == id);

            if (trash != null) inactiveCollection.InsertOne(trash);
        }

        public void Update(string id, Flight flight)
        {
            var activeCollection = Database.GetCollection<Flight>("ActiveAirCraft");
            activeCollection.ReplaceOne(f => f.Id  == id, flight);
        }
    }
}