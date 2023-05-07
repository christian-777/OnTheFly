using MongoDB.Driver;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.Connections
{
    public class FlightConnection
    {
        public IMongoDatabase Database { get; private set; }
        public FlightConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("Flight");
        }

        public Flight Insert(FlightDTO flightDTO)
        {
            // Dados de Company
            #region company
            CompanyDTO companyDTO = flightDTO.Plane.Company;

            DateOnly dtOpen = DateOnly.Parse(companyDTO.DtOpen.Year + "/" + companyDTO.DtOpen.Month + "/" + companyDTO.DtOpen.Day);

            Company company = new Company
            {
                Address = companyDTO.Address,
                Cnpj = companyDTO.Cnpj,
                DtOpen = dtOpen,
                Name = companyDTO.Name,
                NameOPT = companyDTO.NameOPT,
                Status = companyDTO.Status
            };

            #endregion

            // Dados de aircraft
            #region aircraft

            AirCraftDTO airCraftDTO = flightDTO.Plane;

            DateOnly? dtLastFlight;
            if (airCraftDTO.DtLastFlight != null) dtLastFlight = DateOnly.Parse(airCraftDTO.DtLastFlight.Year + "/" + airCraftDTO.DtLastFlight.Month + "/" + airCraftDTO.DtLastFlight.Day);
            else dtLastFlight = null;

            DateOnly dtRegistry = DateOnly.Parse(airCraftDTO.DtRegistry.Year + "/" + airCraftDTO.DtRegistry.Month + "/" + airCraftDTO.DtRegistry.Day);

            AirCraft airCraft = new AirCraft
            {
                Capacity = airCraftDTO.Capacity,
                DtLastFlight = dtLastFlight,
                DtRegistry = dtRegistry,
                Company = company,
                RAB = flightDTO.Plane.RAB
            };
            #endregion

            // Dados de flight
            #region flight
            Flight flight = new Flight
            {
                Destiny = flightDTO.Destiny,
                Plane = airCraft,
                Departure = DateTime.Parse(flightDTO.Departure.ToString("yyyy/MM/dd hh:mm")),
                Status = flightDTO.Status,
                Sales = flightDTO.Sales
            };
            #endregion

            var activeCollection = Database.GetCollection<Flight>("ActiveFlight");

            activeCollection.InsertOne(flight);
            return flight;
        }

        public Flight Delete(string IATA, string RAB, DateTime departure)
        {
            IMongoCollection<Flight> activeCollection = Database.GetCollection<Flight>("ActiveFlight");
            IMongoCollection<Flight> inactiveCollection = Database.GetCollection<Flight>("InactiveFlight");
            Flight? flight = activeCollection.FindOneAndDelete(f => f.Destiny.IATA == IATA && f.Plane.RAB == RAB && f.Departure == departure);

            if (flight != null) inactiveCollection.InsertOne(flight);
            return flight;
        }
    }
}