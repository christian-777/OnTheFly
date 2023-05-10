using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using OnTheFly.Models.DTO;
using OnTheFly.Models;
using MongoDB.Bson;

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
                var collection = Database.GetCollection<Sale>("ActivateSale");
                collection.InsertOne(sale);
                return sale;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        
        public Sale FindSale(string cpf, string iata, string rab, DateTime departure)
        {
            var collection = Database.GetCollection<Sale>("ActivateSale");

            BsonDateTime bsonDate = BsonDateTime.Create(departure);

            var filter =
                    Builders<Sale>.Filter.Eq("Flight.Departure", bsonDate)
                    & Builders<Sale>.Filter.Eq("Flight.Destiny.iata", iata)
                    & Builders<Sale>.Filter.Eq("Flight.Plane.RAB", rab)
                    & Builders<Sale>.Filter.Eq("Passengers.0", cpf);

            return collection.Find(filter).FirstOrDefault();

        }
        

        public List<Sale> FindAll()
        {
            var collection = Database.GetCollection<Sale>("ActivateSale");
            return collection.Find(s => true).ToList();
        }

        
        public bool Update(string cpf, string iata, string rab, DateTime departure, Sale sale)
        {
            var collection = Database.GetCollection<Sale>("ActivateSale");

            BsonDateTime bsonDate = BsonDateTime.Create(departure);

            var filter =
                    Builders<Sale>.Filter.Eq("Flight.Departure", bsonDate)
                    & Builders<Sale>.Filter.Eq("Flight.Destiny.iata", iata)
                    & Builders<Sale>.Filter.Eq("Flight.Plane.RAB", rab)
                    & Builders<Sale>.Filter.Eq("Passengers.0", cpf);

            var updateReserve = Builders<Sale>.Update.Set("Reserved", !sale.Reserved);
            var updateSale = Builders<Sale>.Update.Set("Sold", !sale.Sold);

            if (collection.UpdateOne(filter, updateReserve).IsAcknowledged && collection.UpdateOne(filter, updateSale).IsAcknowledged)
                return true;
            else
                return false;
        }
        
        public bool Delete(string cpf, string iata, string rab, DateTime departure)
        {
            bool status = false;
            try
            {
                var collection = Database.GetCollection<Sale>("ActivateSale");
                var collectionDeleted = Database.GetCollection<Sale>("DeletedSale");

                BsonDateTime bsonDate = BsonDateTime.Create(departure);

                var filter =
                    Builders<Sale>.Filter.Eq("Flight.Departure", bsonDate)
                    & Builders<Sale>.Filter.Eq("Flight.Destiny.iata", iata)
                    & Builders<Sale>.Filter.Eq("Flight.Plane.RAB", rab)
                    & Builders<Sale>.Filter.Eq("Passengers.0", cpf);

                Sale? trash = collection.FindOneAndDelete(filter);
                if (trash == null) return false;

                collectionDeleted.InsertOne(trash);
                
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
