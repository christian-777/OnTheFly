using System;
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
        public Passenger Insert(Passenger passenger)
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            collection.InsertOne(passenger);
            var pass= collection.Find(p=>p.CPF==passenger.CPF).FirstOrDefault();
            return pass;
        }
        public Passenger FindPassenger(string cpf)
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            var passenger = collection.Find(c => c.CPF == cpf).FirstOrDefault();
            return passenger;

        }

        public Passenger FindPassengerRestrict(string cpf)
        {
            var collection = Database.GetCollection<Passenger>("RestrictPassenger");
            var restrictPassenger = collection.Find(p => p.CPF == cpf).FirstOrDefault();
            return restrictPassenger;
        }


        public List<Passenger> FindAll()
        {
            var collection = Database.GetCollection<Passenger>("ActivePassenger");
            var passengers= collection.Find(p => true).ToList();
            return passengers;
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

                var trash = collection.FindOneAndDelete(p => p.CPF == cpf);

                collectionofdelete.InsertOne(trash);

                status = true;

            }catch
            {
                status = false;
            }
            return status;
        }
    }
}
