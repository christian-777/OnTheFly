using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OnTheFly.Models
{
    public class Sale
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Flight Flight { get; set; }
        public List<string> Passengers  { get; set; }
        public bool Reserved { get; set; }
        public bool Sold { get; set; }
    }
}
