using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OnTheFly.Models.DTO
{
    public class FlightDTO
    {
        public Airport Destiny { get; set; }
        public AirCraftDTO Plane { get; set; }
        public int Sales { get; set; }
        public DateTime Departure { get; set; }
        public bool Status { get; set; }
    }
}
