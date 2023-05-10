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
        [StringLength(18)]
        public string Cnpj { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(30)]
        public string NameOPT { get; set; }

        public DateDTO DtOpen { get; set; }

        public bool? Status { get; set; }

        public string Zipcode { get; set; }

        public string Street { get; set; }

        public int Number { get; set; }
    }
}
