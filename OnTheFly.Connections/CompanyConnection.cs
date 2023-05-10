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

        public Company Insert(Company company)
        {
            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            collection.InsertOne(company);
            var com = collection.Find(c => c.Cnpj == company.Cnpj).FirstOrDefault();
            return com;
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

        public Company FindByCnpjDeleted(string cnpj)
        {
            var collection = _dataBase.GetCollection<Company>("DeletedCompanies");
            return collection.Find(x => x.Cnpj == cnpj).FirstOrDefault();
        }

        public bool Delete(string cnpj)
        {
            try
            {
                var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
                var collectioRestricted = _dataBase.GetCollection<Company>("RestrictedCompanies");
                var collectionDeleted = _dataBase.GetCollection<Company>("DeletedCompanies");

                var trash = collection.FindOneAndDelete(c => c.Cnpj == cnpj);
                if (trash == null)
                {
                    var trashRestricted = collectioRestricted.FindOneAndDelete(c => c.Cnpj == cnpj);
                    if (trashRestricted == null)
                    {
                        return false;
                    }
                    else
                    {
                        collectionDeleted.InsertOne(trashRestricted);
                        return true;
                    }
                }
                else
                {
                    collectionDeleted.InsertOne(trash);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Restrict(string cnpj)
        {
            try
            {
                var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
                var collectioRestricted = _dataBase.GetCollection<Company>("RestrictedCompanies");

                var company = collection.FindOneAndDelete(c => c.Cnpj == cnpj);
                if (company == null)
                {
                    return false;
                }
                else
                {
                    collectioRestricted.InsertOne(company);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Unrestrict(string cnpj)
        {
            try
            {
                var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
                var collectioRestricted = _dataBase.GetCollection<Company>("RestrictedCompanies");

                var restricted = collectioRestricted.FindOneAndDelete(c => c.Cnpj == cnpj);
                if (restricted == null)
                {
                    return false;
                }
                else
                {
                    collection.InsertOne(restricted);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UndeleteCompany(string cnpj)
        {
            try
            {
                var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
                var collectioDeleted = _dataBase.GetCollection<Company>("DeletedCompanies");

                var deleted = collectioDeleted.FindOneAndDelete(c => c.Cnpj == cnpj);
                if (deleted == null)
                {
                    return false;
                }
                else
                {
                    collection.InsertOne(deleted);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Update(string cnpj, Company company)
        {
            var collection = _dataBase.GetCollection<Company>("ActivatedCompanies");
            return collection.ReplaceOne(c=> c.Cnpj==cnpj, company).IsAcknowledged;
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