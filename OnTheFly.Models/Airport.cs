using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OnTheFly.Models
{
    [BsonIgnoreExtraElements]
    public class Airport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("iata")]
        [StringLength(3)]
        public string IATA { get; set; }

        [BsonElement("state")]
        public string State { get; set; }

        [BsonElement("city")]
        [StringLength(20)]
        public string City { get; set; }

        [BsonElement("country_id")]
        [StringLength(2)]
        public string Country { get; set; }
    }
}
