using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly.Models.DTO
{
    public class CompanyDTO
    {
        public string Id { get; set; }
        [StringLength(14)]
        public string Cnpj { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(30)]
        public string NameOPT { get; set; }

        public DateTime DtOpen { get; set; }

        public bool? Status { get; set; }

        public Address Address { get; set; }

        public AirCraftDTO AirCraftDTO { get; set; }
    }
}
