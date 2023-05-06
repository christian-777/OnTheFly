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
                DtRegistry = DateOnly.Parse("1990/10/20"),
                DtLastFlight = DateOnly.Parse("2024/05/02"),
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

        [Fact]
        public void Delete() 
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
                DtRegistry = DateOnly.Parse("1990/10/20"),
                DtLastFlight = DateOnly.Parse("2024/05/02"),
                Company = new Company()
            };

            DateTime departure = DateTime.Parse("2023-05-06T16:18:41.766+00:00");

            _flight.Delete(airport, airCraft, departure);
        }

        [Fact]
        public void Update()
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

            DateTime departure = DateTime.Parse("2023-05-06T16:18:41.766+00:00");

            _flight.Update(airport, airCraft, departure, new Flight { Sales = 30});
        }
    }
}