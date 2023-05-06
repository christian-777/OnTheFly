namespace OnTheFly.Models
{
    public class Sale
    {
        public Flight Flight { get; set; }
        public List<Passenger> Passengers  { get; set; }
        public bool Reserved { get; set; }
        public bool Sold { get; set; }
    }
}
