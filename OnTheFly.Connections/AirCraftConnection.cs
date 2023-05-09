using MongoDB.Driver;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.Connections
{
    public class AirCraftConnection
    {
        public IMongoDatabase Database { get; private set; }
        public AirCraftConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("AirCraft");
        }

        public AirCraft Insert(AirCraftDTO airCraftDTO)
        {
            Company company = new Company()
            {
                Id = airCraftDTO.Company.Id,
                Address = airCraftDTO.Company.Address,
                Cnpj = airCraftDTO.Company.Cnpj,
                DtOpen = airCraftDTO.Company.DtOpen,
                Name= airCraftDTO.Company.Name,
                NameOPT= airCraftDTO.Company.NameOPT,
                Status = airCraftDTO.Company.Status
            };
            AirCraft airCraft = new AirCraft()
            {
                Capacity = airCraftDTO.Capacity,
                Company = company,
                RAB = airCraftDTO.RAB,
                DtRegistry = DateOnly.Parse(airCraftDTO.DtRegistry.Year + "/" + airCraftDTO.DtRegistry.Month + "/" + airCraftDTO.DtRegistry.Day)
            };
            if (airCraftDTO.DtLastFlight != null)
                airCraft.DtLastFlight = DateOnly.Parse(airCraftDTO.DtLastFlight.Year + "/" + airCraftDTO.DtLastFlight.Month + "/" + airCraftDTO.DtLastFlight.Day);
            
                    var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.InsertOne(airCraft);

            return collection.Find(a => a.RAB == airCraftDTO.RAB).First();
        }

        public List<AirCraft>? FindAll()
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            return collection.Find<AirCraft>(a=>true).ToList();
        }

        public AirCraft? FindOne(string rab)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            return collection.Find<AirCraft>(a => a.RAB==rab).First();
        }

        public AirCraft Delete(string rab)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            var collection2 = Database.GetCollection<AirCraft>("InactiveAirCraft");

            var trash= collection.FindOneAndDelete(a => a.RAB == rab);
            collection2.InsertOne(trash);

            return trash;
        }

        public AirCraft? Update(string rab, AirCraft airCraft)
        {
            var collection = Database.GetCollection<AirCraft>("ActiveAirCraft");
            collection.ReplaceOne(a => a.RAB == rab, airCraft);
            return collection.Find(a => a.RAB == airCraft.RAB).First();
        }
    }
}