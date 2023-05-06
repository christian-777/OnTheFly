using OnTheFly.Connections;
using OnTheFly.Models;

namespace OnTheFly.Test
{
    public class FlightTest
    {
        private readonly FlightConnection _flight;
        public FlightTest()
        {
            _flight = new FlightConnection();
        }

        [Fact]
        public void Insert()
        {
            Airport airport = new Airport
            {
                IATA = "CGH",
                State = "SP",
                City = "São Paulo",
                Country = "BR"
            };

            AirCraft airCraft = new AirCraft
            {
                RAB = "DTCANC",
                Capacity = 20,
                DtRegistry = DateOnly.Parse("2000/10/20"),
                DtLastFlight = DateOnly.Parse("2023/05/02"),
                Company = new Company()
            };

            Flight flight = new Flight
            {
                Destiny = airport,
                Plane = airCraft,
                Sales = 20,
                Departure = DateTime.Now,
                Status = true
            };

            _flight.Insert(flight);
        }
    }
}