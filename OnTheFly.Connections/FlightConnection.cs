using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

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

        public Flight Insert(FlightDTO flightDTO, AirCraft aircraft, Airport airport, DateTime date)
        {
            // Dados de flight
            #region flight
            Flight flight = new Flight
            {
                Destiny = airport,
                Plane = aircraft,
                Departure = date,
                Status = flightDTO.Status,
                Sales = flightDTO.Sales
            };
            #endregion

            var activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            activeCollection.InsertOne(flight);
            return flight;
        }

        public List<Flight> FindAll()
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");
            return activeCollection.Find(f => true).ToList();
        }

        public Flight? Get(string IATA, string RAB, BsonDateTime departure)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");
            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA) & Builders<Flight>.Filter.Eq("Plane.RAB", RAB) & Builders<Flight>.Filter.Eq("Departure", departure);
            return activeCollection.Find(filter).FirstOrDefault();
        }

        public bool UpdateSales(string IATA, string RAB, BsonDateTime departure, int salesNumber)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA)
                & Builders<Flight>.Filter.Eq("Plane.RAB", RAB)
                & Builders<Flight>.Filter.Eq("Departure", departure);

            if (activeCollection.Find(filter) == null) return false;
            var update = Builders<Flight>.Update.Set("Sales", salesNumber);

            return activeCollection.UpdateOne(filter, update).IsAcknowledged;
        }

        public bool UpdateStatus(string IATA, string RAB, BsonDateTime departure)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA)
                & Builders<Flight>.Filter.Eq("Plane.RAB", RAB)
                & Builders<Flight>.Filter.Eq("Departure", departure);

            Flight? flight = activeCollection.Find(filter).FirstOrDefault();

            if (flight == null) return false;

            if (flight.Status == false) return false;

            var update = Builders<Flight>.Update.Set("Status", false);

            return activeCollection.UpdateOne(filter, update).IsAcknowledged;
        }

        public bool Delete(string IATA, string RAB, BsonDateTime departure)
        {
            // Troca de collection
            IMongoCollection<Flight> collection = Database.GetCollection<Flight>("ActivatedFlight");
            IMongoCollection<Flight> collectionDeleted = Database.GetCollection<Flight>("DeletedFlight");

            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA)
                & Builders<Flight>.Filter.Eq("Plane.RAB", RAB)
                & Builders<Flight>.Filter.Eq("Departure", departure);

            if (collection.Find(filter) == null) return false;

            Flight? flight = collection.FindOneAndDelete(filter);
            if (flight == null) return false;

            collectionDeleted.InsertOne(flight);
            return true;
        }
    }
}