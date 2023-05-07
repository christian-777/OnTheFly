﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.Connections
{
    public class PassengerConnection
    {
        public IMongoDatabase Database { get; private set; }

        public PassengerConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("Passenger");
        }
        public Passenger Insert(PassengerDTO passengerdto)
        {
            Passenger passenger = new()
            {
                Address = passengerdto.Address,
                CPF = passengerdto.CPF,
                Name = passengerdto.Name,
                Gender = passengerdto.Gender,
                Phone = passengerdto.Phone,
                DtBirth = DateOnly.Parse(passengerdto.DtBirth.Year + "/" + passengerdto.DtBirth.Month + "/" + passengerdto.DtBirth.Day),
                DtRegister = passengerdto.DtRegister,
                Status = passengerdto.Status
            };

            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            collection.InsertOne(passenger);
            return passenger;
        }
        public Passenger FindPassenger(string cpf)
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            return collection.Find(cpf).FirstOrDefault();

        }
        public List<Passenger> FindAll()
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            return collection.Find(p => true).ToList();
        }
        public Passenger Update(string cpf, Passenger passenger)
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            collection.ReplaceOne(p => p.CPF == cpf, passenger);
            return passenger;
        }
        public bool Delete(string cpf)
        {
            bool status = false;
            try
            {
                var collection = Database.GetCollection<Passenger>("ActivePassenger");
                var collectionofdelete = Database.GetCollection<Passenger>("InactivePassenger");

                var trash = collection.Find(cpf).FirstOrDefault();

                collectionofdelete.InsertOne(trash);
                collection.FindOneAndDelete(p => p.CPF == cpf);

                status = true;

            }catch
            {
                status = false;
            }
            return status;
        }
    }
}