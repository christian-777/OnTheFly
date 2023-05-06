using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class AirCraft
    {
        [StringLength(6)]
        public string RAB { get; set; }
        public int Capacity { get; set; }
        public DateOnly DtRegistry { get; set; }
        public DateOnly? DtLastFlight { get; set; }
        public Company Company { get; set; }
    }
}
