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
        public FlightDTO Flight { get; set; }
        public List<PassengerDTO> Passengers { get; set; }
        public bool Reserved { get; set; }
        public bool Sold { get; set; }
    }
}
