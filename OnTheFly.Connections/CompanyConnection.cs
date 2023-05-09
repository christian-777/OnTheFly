using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

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

        public Company Insert(CompanyDTO companyDTO)
        {
            Company company = new Company()
            {
                Address = companyDTO.Address,
                Cnpj = companyDTO.Cnpj,
                DtOpen = companyDTO.DtOpen,
                Name = companyDTO.Name,
                NameOPT = companyDTO.NameOPT,
                Status = companyDTO.Status
            };
            
             var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
             collection.InsertOne(company);

            return company;
        }

        public List<Company> FindAll()
        {
            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            return collection.Find(x => true).ToList();
        }

        public Company FindByCnpj(string cnpj)
        {
            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            return collection.Find(x => x.Cnpj == cnpj).FirstOrDefault();
        }

        public Company FindByCnpjRestricted(string cnpj)
        {
            var collection = _dataBase.GetCollection<Company>("RestrictedCompanies");
            return collection.Find(x => x.Cnpj == cnpj).FirstOrDefault();
        }

        public void Delete(string cnpj)
        {
            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            var collection2 = _dataBase.GetCollection<Company>("DeletedCompanies");

            var trash = collection.Find<Company>(x => x.Cnpj == cnpj).FirstOrDefault();

            collection2.InsertOne(trash);

            collection.DeleteOne(x => x.Cnpj == cnpj);
        }

        public void DeleteByRestricted(string cnpj)
        {
            var collection = _dataBase.GetCollection<Company>("RestrictedCompanies");
            var collection2 = _dataBase.GetCollection<Company>("DeletedCompanies");

            var trash = collection.Find<Company>(x => x.Cnpj == cnpj).FirstOrDefault();

            collection2.InsertOne(trash);

            collection.DeleteOne(x => x.Cnpj == cnpj);
        }

        public void UpdateNameOPT(string cnpj, string nameOPT)
        {
            var filter= Builders<Company>.Filter.Eq("Cnpj", cnpj);
            var update= Builders<Company>.Update.Set("NameOPT", nameOPT);

            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            collection.UpdateOne(filter, update);
        }

        public void UpdateStatus(string cnpj, bool status)
        {
            var filter = Builders<Company>.Filter.Eq("Cnpj", cnpj);
            var update = Builders<Company>.Update.Set("Status", status);

            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            collection.UpdateOne(filter, update);
        }
    }
}