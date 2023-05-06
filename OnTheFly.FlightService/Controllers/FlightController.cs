using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnTheFly.Connections;
using OnTheFly.FlightService.Services;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.FlightService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private FlightConnection _flight;
        private AirportServices _airport;

        public FlightController(FlightConnection flight, AirportServices airport)
        {
            _flight = flight;
            _airport = airport;
        }

        [HttpPost]
        public ActionResult Insert(FlightDTO flightDTO)
        {
            if (flightDTO == null) return NoContent();

            Airport? airport = _airport.GetValidDestiny(flightDTO.Destiny.IATA).Result;
            if (airport == null) return NotFound();
            if (airport.Country != "BR") return Unauthorized();

            flightDTO.Destiny = airport;

            Flight? flight = _flight.Insert(flightDTO);

            if (flight == null) return BadRequest();
            return Ok();
        }

        [HttpDelete("{IATA:length(3)}, {RAB:length(6)}, {departure}")]
        public ActionResult Delete(string IATA, string RAB, DateTime? departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();
            Flight? flight = _flight.Delete(IATA, RAB, departure.Value);

            if (flight == null) return NotFound();
            return Ok();
        }
    }
}
