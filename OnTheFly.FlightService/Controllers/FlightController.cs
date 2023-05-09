using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
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
        private Services.AirportService _airport;
        private Services.AircraftService _aircraft;

        public FlightController(FlightConnection flight, Services.AirportService airport, AircraftService aircraft)
        {
            _flight = flight;
            _airport = airport;
            _aircraft = aircraft;
        }

        [HttpGet("{IATA}, {RAB}, {departure}")]
        public ActionResult<string> Get(string IATA, string RAB, string departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();

            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            BsonDateTime departureBSON = BsonDateTime.Create(departureDT.Date);
            if (departureBSON == null) return BadRequest("Formato de data não reconhecido");

            Flight? flight = _flight.Get(IATA, RAB, departureBSON);

            if (flight == null) return NotFound();

            return JsonConvert.SerializeObject(flight, Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(FlightDTO flightDTO)
        {
            if (flightDTO == null) return NoContent();

            // Verificar se airport existe e é válido
            Airport? airport = _airport.GetValidDestiny(flightDTO.Destiny.IATA).Result;
            if (airport == null) return NotFound();
            if (airport.Country != "BR") return Unauthorized();

            flightDTO.Destiny = airport;

            // Verificar se aircraft existe e é válido
            AirCraft? aircraft = _aircraft.GetAircraft(flightDTO.Plane.RAB).Result;
            if (aircraft == null) return NotFound();
            if (aircraft.Company == null) return NotFound();
            if (aircraft.Company.Status == false || aircraft.Company.Status == null) return Unauthorized();

            // Verificação se data de voo é depois do último voo do aircraft
            if (aircraft.DtLastFlight != null && aircraft.DtLastFlight > flightDTO.Departure)
                return BadRequest("Data de voo não pode ser antes do último voo do avião");

            // Atualizar data de último voo de aircraft para a data do voo
            aircraft.DtLastFlight = flightDTO.Departure;
            if (_aircraft.UpdateAircraft(aircraft.RAB, flightDTO.Departure) == null) return BadRequest("Impossível atualizar última data de voo do avião");

            // Inserção de flight
            Flight? flight = _flight.Insert(flightDTO, aircraft, airport);

            if (flight == null) return BadRequest();
            return Ok();
        }

        [HttpPatch("{IATA}, {RAB}, {departure}, {salesNumber}")]
        public ActionResult PatchSales(string IATA, string RAB, DateTime departure, int salesNumber)
        {
            Flight? flight = _flight.Get(IATA, RAB, departure);
            if (flight == null) return NotFound();

            _flight.PatchSalesNumber(IATA, RAB, departure, salesNumber);
            return Ok();
        }

        [HttpDelete("{IATA}, {RAB}, {departure}")]
        public ActionResult Delete(string IATA, string RAB, DateTime? departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();
            Flight? flight = _flight.Delete(IATA, RAB, departure.Value);

            if (flight == null) return NotFound();
            return Ok();
        }
    }
}
