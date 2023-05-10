﻿using Microsoft.VisualBasic;
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
                Departure = flightDTO.Departure,
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
            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA) & Builders<Flight>.Filter.Eq("Plane.RAB", RAB) & Builders<Flight>.Filter.Eq("Departure", departure);
            return activeCollection.Find(filter).FirstOrDefault();
        }

        public void PatchSalesNumber(string IATA, string RAB, BsonDateTime departure, int salesNumber)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActivatedFlight");

            var filter = Builders<Flight>.Filter.Eq("Destiny.iata", IATA)
                & Builders<Flight>.Filter.Eq("Plane.RAB", RAB)
                & Builders<Flight>.Filter.Eq("Departure", departure);

            var res = activeCollection.Find(filter);

            var update = Builders<Flight>.Update.Set("Sales", salesNumber);
            activeCollection.UpdateOne(filter, update);
        }

        public Flight Delete(string IATA, string RAB, BsonDateTime departure)
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