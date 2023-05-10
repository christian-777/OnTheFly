using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly.Models.DTO
{
    public class AirCraftDTO
    {
        [StringLength(6)]
        public string RAB { get; set; }
        public int Capacity { get; set; }
        public DateDTO DtRegistry { get; set; }
        public DateDTO? DtLastFlight { get; set; }
        public string Company { get; set; }
    }
}
