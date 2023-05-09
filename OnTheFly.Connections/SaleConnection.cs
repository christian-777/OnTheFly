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
        public Sale Insert(Sale sale)
        {
            try
            {
                var collection = Database.GetCollection<Sale>("ActiveSale");
                collection.InsertOne(sale);
                return sale;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        
        public Sale FindSale(string cpf, string iata, string rab, DateTime departure)
        {
            var collection = Database.GetCollection<Sale>("ActiveSale");
            return collection.Find(s=> (s.Flight.Departure==departure) && (s.Flight.Plane.RAB==rab) && (s.Flight.Destiny.IATA==iata) && (s.Passengers.FindAll(p=> p.CPF == cpf)[0].CPF==cpf)).FirstOrDefault();

        }
        

        public List<Sale> FindAll()
        {
            var collection = Database.GetCollection<Sale>("ActiveSale");
            return collection.Find(s => true).ToList();
        }

        
        public bool Update(string cpf, string iata, string rab, DateTime departure, Sale sale)
        {
            var collection = Database.GetCollection<Sale>("ActiveSale");
            if (collection.ReplaceOne(s => (s.Flight.Departure == departure) && (s.Flight.Plane.RAB == rab) && (s.Flight.Destiny.IATA == iata) && (s.Passengers.FindAll(p => p.CPF == cpf)[0].CPF == cpf), sale).IsModifiedCountAvailable)
                return true;
            else
                return false;
        }
        
        public bool Delete(string cpf, string iata, string rab, DateTime departure)
        {
            bool status = false;
            try
            {
                var collection = Database.GetCollection<Sale>("ActiveSale");
                var collectionofdelete = Database.GetCollection<Sale>("DeletedSale");

                var deletesale= collection.FindOneAndDelete(s => (s.Flight.Departure == departure) && (s.Flight.Plane.RAB == rab) && (s.Flight.Destiny.IATA == iata) && (s.Passengers.FindAll(p => p.CPF == cpf)[0].CPF == cpf));
                collectionofdelete.InsertOne(deletesale);
                
                status = true;

            }
            catch
            {
                status = false;
            }
            return status;
        }
    }
}
