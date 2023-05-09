using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models.DTO
{
    public class PassengerDTO
    {
        public string Id { get; set; }
        [StringLength(14)]
        public string CPF { get; set; }
        [StringLength(30)]
        public string Name { get; set; }
        [StringLength(1)]
        public string Gender { get; set; }
        [StringLength(14)]
        public string? Phone { get; set; }
        public DateTime DtBirth { get; set; }
        public DateTime DtRegister { get; set; }
        public bool Status { get; set; }
        public string Zipcode { get; set; }
        public string Street { get; set; }
        public int Number { get; set; }

    } 
}
