using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class Company
    {
        [StringLength(19)]
        public string Cnpj { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(30)]
        public string NameOPT { get; set; }

        public DateOnly DtOpen { get; set; }

        public bool? Status { get; set; }

        public Address Address { get; set; }
    }
}
