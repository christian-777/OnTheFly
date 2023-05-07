using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using OnTheFly.Models.DTO;
using OnTheFly.Models;

namespace OnTheFly.Connections
{
    public class SaleConnection
    {
        public IMongoDatabase Database { get; private set; }

        public SaleConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("Sale");
        }
        public Sale Insert(SaleDTO saleDTO, Flight flight, List<Passenger> passengers)
        {
            Sale sale = new Sale 
            { 
                Id = saleDTO.Id,
                Flight = flight,
                Passengers = passengers,
                Reserved = saleDTO.Reserved,
                Sold = saleDTO.Sold
            };

            var collection = Database.GetCollection<Sale>("ActiveSale");
            collection.InsertOne(sale);
            return sale;
        }
        /*
        public Sale FindSale(string cpf)
        {
            var collection = Database.GetCollection<Passenger>("ActiveSale");
            return collection.Find(cpf).FirstOrDefault();

        }
        */

        public List<Sale> FindAll()
        {
            var collection = Database.GetCollection<Sale>("ActiveSale");
            return collection.Find(s => true).ToList();
        }

        /*
        public Sale Update(string cpf, Passenger passenger)
        {
            var collection = Database.GetCollection<Passenger>("ActiveSale");
            collection.ReplaceOne(p => p.CPF == cpf, passenger);
            return passenger;
        }
        */

        /*
        public bool Delete(string cpf)
        {
            bool status = false;
            try
            {
                var collection = Database.GetCollection<Passenger>("ActiveSale");
                var collectionofdelete = Database.GetCollection<Passenger>("InactiveActiveSale");

                var trash = collection.Find(cpf).FirstOrDefault();

                collectionofdelete.InsertOne(trash);
                collection.FindOneAndDelete(p => p.CPF == cpf);

                status = true;

            }
            catch
            {
                status = false;
            }
            return status;
        }
        */
    }
}
