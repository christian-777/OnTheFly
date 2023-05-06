using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnTheFly.Models
{
    public class AirCraft
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [StringLength(6)]
        public string RAB { get; set; }
        public int Capacity { get; set; }
        public DateOnly DtRegistry { get; set; }
        public DateOnly? DtLastFlight { get; set; }
        public Company Company { get; set; }
    }
}
