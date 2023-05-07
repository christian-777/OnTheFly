using MongoDB.Bson;
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
                DtOpen = DateOnly.Parse(companyDTO.DtOpen.Year + "/" + companyDTO.DtOpen.Month + "/" + companyDTO.DtOpen.Day),
                Name = companyDTO.Name,
                NameOPT = companyDTO.NameOPT,
                Status = companyDTO.Status
            };
            
             var collection = _dataBase.GetCollection<Company>("ActiveCompany");
             collection.InsertOne(company);

            return company;
        }

        public List<Company> FindAll()
        {
            var collection = _dataBase.GetCollection<Company>("ActiveCompany");
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