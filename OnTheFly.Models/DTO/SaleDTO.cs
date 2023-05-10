using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly.Models.DTO
{
    public class SaleDTO
    {
        public string Id { get; set; }
        public string IATA { get; set; }
        public string RAB { get; set; }
        public DateDTO Departure { get; set; }
        public List<string> Passengers { get; set; }
        public bool Reserved { get; set; }
        public bool Sold { get; set; }
    }
}
