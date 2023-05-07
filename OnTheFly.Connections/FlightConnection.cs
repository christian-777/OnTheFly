﻿using MongoDB.Driver;
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

            var activeCollection = Database.GetCollection<Flight>("ActiveFlight");

            activeCollection.InsertOne(flight);
            return flight;
        }

        public Flight? Get(string IATA, string RAB, DateTime departure)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            return activeCollection.Find(f => f.Destiny.IATA == IATA && f.Plane.RAB == RAB && f.Departure == departure).FirstOrDefault();
        }

        public Flight Delete(string IATA, string RAB, DateTime departure)
        {
            // Troca de collection
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            IMongoCollection<Flight> inactiveCollection = Database.GetCollection<Flight>("InactiveFlight");
            Flight? flight = activeCollection.FindOneAndDelete(f => f.Destiny.IATA == IATA && f.Plane.RAB == RAB && f.Departure == departure);

            // Trocando status para cancelado (false)
            flight.Status = false;
            if (flight != null) inactiveCollection.InsertOne(flight);
            return flight;
        }
    }
}