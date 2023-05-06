using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class Airport
    {
        [StringLength(3)]
        public string IATA { get; set; }
        [StringLength(2)]
        public string State { get; set; }
        [StringLength(20)]
        public string City { get; set; }
        [StringLength(2)]
        public string Country { get; set; }
    }
}
