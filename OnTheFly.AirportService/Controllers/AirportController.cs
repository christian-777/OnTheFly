using OnTheFly.AirportService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using OnTheFly.Connections;
using OnTheFly.Models;

namespace OnTheFly.AirportService.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class AirportController : ControllerBase
    {
        private readonly AirportConnection _airport;
        private readonly StateConnection _state;
        public AirportController(AirportConnection airport, StateConnection state)
        {
            _airport = airport;
            _state = state;
        }

        [HttpGet("/ByIATA/{iata}", Name = "GetAirportIata")]
        public ActionResult<Airport> Get(string iata)
        {
            Airport? airport = _airport.Get(iata);
            if (airport == null)
                return NotFound();

            State? state = _state.GetUF(airport.State);
            if (state == null) airport.State = "null";
            else airport.State = state.UF;

            return airport;
        }
        
        [HttpGet("/ByState/{state}", Name = "GetAirportState")]
        public ActionResult<List<Airport>> GetByState(string state)
        {
            var airport = _airport.GetByState(state);

            if (airport.Count == 0)
                return NotFound();

            return airport;
        }

        [HttpGet("/ByCityName/{city}", Name = "GetAirportCityName")]
        public ActionResult<List<Airport>> GetByCityName(string city)
        {
            var airport = _airport.GetByCityName(city);

            if (airport.Count == 0)
                return NotFound();

            return airport;
        }

        [HttpGet("/ByCountry/{country}", Name = "GetAirportCountry")]
        public ActionResult<List<Airport>> GetByCountry(string country)
        {
            var airport = _airport.GetByCountry(country);

            if (airport.Count == 0)
                return NotFound();

            return airport;
        }
    }
}
