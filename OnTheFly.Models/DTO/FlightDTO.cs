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
        public string IATA { get; set; }
        public string RAB { get; set; }
        public int Sales { get; set; }
        public DateDTO Departure { get; set; }
        public bool Status { get; set; }
    }
}
