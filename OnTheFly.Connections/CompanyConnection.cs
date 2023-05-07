using MongoDB.Bson;
using MongoDB.Driver;
using OnTheFly.Models;

namespace OnTheFly.Connections
{
    public class CompanyConnection
    {
        private IMongoDatabase _dataBase;

        public CompanyConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _dataBase = client.GetDatabase("Company");
        }

        public bool Insert(Company company)
        {
            bool status = false;
            if (company.Status is false)
            {
                var collection = _dataBase.GetCollection<Company>("Restricted Company");
                collection.InsertOne(company);
                status = true;
            }

            if ((company.Status is true) || (company.Status is null))
            {
                var collection = _dataBase.GetCollection<Company>("Unrestricted Company");
                collection.InsertOne(company);
                status = true;
            }
            return status;
        }

        public List<Company> FindAll()
        {
            var collection = _dataBase.GetCollection<Company>("Unrestricted Company");
            return collection.Find(x => true).ToList();
        }

        //public void Delete(string cnpj)
        //{
        //    var collection = _dataBase.GetCollection<Company>("");
        //    var collection2 = _dataBase.GetCollection<Company>("");

        //    var trash = collection.Find<Company>().FirstOrDefault();

        //    collection2.InsertOne(trash);

        //    collection.FindOneAndDelete();
        //}

        //public void Update()
        //{
        //    var collection = _dataBase.GetCollection<Company>("");
        //    collection.ReplaceOne();
        //}
    }
}