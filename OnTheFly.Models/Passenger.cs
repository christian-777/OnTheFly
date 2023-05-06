using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class Passenger
    {
        [StringLength(14)]
        public string CPF { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        public char Gender { get; set; }
        [StringLength(14)]
        public string? Phone { get; set; }
        public DateOnly DtBirth { get; set; }
        public DateTime DtRegister { get; set; }
        public bool? Status { get; set; }
        public Address Address { get; set; }
    }
}
