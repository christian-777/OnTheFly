using MongoDB.Driver;
using System.Collections.Generic;
using OnTheFly.Models;

namespace OnTheFly.AirportService.Services
{
    public class AirportConnection

    {
        private readonly IMongoCollection<Airport> Collection;

        public AirportConnection()
        {
            IMongoClient airport = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = airport.GetDatabase("Airport");
            Collection = database.GetCollection<Airport>("Airports");
        }

        public List<Airport> Get() =>
            Collection.Find(airport => true).ToList();

        public Airport? Get(string iata) =>
            Collection.Find<Airport>(airport => airport.IATA == iata).FirstOrDefault();

        public List<Airport> GetByState(string state) =>
            Collection.Find<Airport>(airport => airport.State == state).ToList();

        public List<Airport> GetByCityName(string city) =>
            Collection.Find<Airport>(airport => airport.City == city).ToList();

        public List<Airport> GetByCountry(string country_id) =>
            Collection.Find<Airport>(airport => airport.Country == country_id).ToList();
            
    }
}