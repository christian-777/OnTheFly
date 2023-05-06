using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class Address
    {
        [StringLength(9)]
        public string Zipcode { get; set; }
        [StringLength(100)]
        public string? Street { get; set; }
        [StringLength(10)]
        public string? Complement { get; set; }
        [StringLength(30)]
        public string City { get; set; }
        [StringLength(2)]
        public string State { get; set; }
        public int Number { get; set; }
    }
}