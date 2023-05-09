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

        public Flight Insert(FlightDTO flightDTO, AirCraft aircraft, Airport airport)
        {
            // Dados de flight
            #region flight
            Flight flight = new Flight
            {
                Destiny = airport,
                Plane = aircraft,
                Departure = DateTime.Parse(flightDTO.Departure.ToString("yyyy/MM/dd hh:mm")),
                Status = flightDTO.Status,
                Sales = flightDTO.Sales
            };
            #endregion

            var activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            activeCollection.InsertOne(flight);
            return flight;
        }

        public Flight? Get(string IATA, string RAB, BsonDateTime departure)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");
            return activeCollection.Find(f => f.Destiny.IATA == IATA && f.Plane.RAB == RAB && f.Departure == departure).FirstOrDefault();
        }

        public void PatchSalesNumber(string IATA, string RAB, DateTime departure, int salesNumber)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ";
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            string jsonDate = JsonConvert.SerializeObject(departure, jsonSettings);

            var filter = Builders<Flight>.Filter.Eq("IATA", IATA)
                & Builders<Flight>.Filter.Eq("RAB", RAB)
                & Builders<Flight>.Filter.Eq("Departure", jsonDate);

            var update = Builders<Flight>.Update.Set("Sales", salesNumber);
            activeCollection.UpdateOne(filter, update);
        }

        public Flight Delete(string IATA, string RAB, DateTime departure)
        {
            // Troca de collection
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");
            IMongoCollection<Flight> inactiveCollection = Database.GetCollection<Flight>("DeletedFlight");

            Flight? flight = activeCollection.FindOneAndDelete(f => f.Plane.RAB == RAB && f.Destiny.IATA == IATA && f.Departure == departure);

            if (flight != null) inactiveCollection.InsertOne(flight);
            return flight;
        }
    }
}