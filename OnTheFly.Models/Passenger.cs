using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OnTheFly.Models
{
    public class Passenger
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [StringLength(14)]
        public string CPF { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        public char Gender { get; set; }
        [StringLength(14)]
        public string? Phone { get; set; }
        public DateOnly DtBirth { get; set; }
        public DateTime DtRegister { get; set; }
        public bool Status { get; set; }
        public Address Address { get; set; }
    }
}
