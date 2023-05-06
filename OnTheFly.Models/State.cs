using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace OnTheFly.Models
{
    [BsonIgnoreExtraElements]
    public class State
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("uf")]
        public string UF { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}
