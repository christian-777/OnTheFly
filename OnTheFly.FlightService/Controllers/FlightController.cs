using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.FlightService.Services;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using ThirdParty.Json.LitJson;

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

        [HttpPost("/Get/{IATA}, {RAB}")]
        public ActionResult<string> Get(string IATA, string RAB,DateDTO departure)
        {
            DateTime date;
            try
            {
                date = DateTime.Parse(departure.Year + "/" + departure.Month + "/" + departure.Day +" 09:00");
            }
            catch
            {
                return BadRequest("Data invalida");
            }
            if (DateTime.Now.Subtract(date).TotalDays < 0)
                return BadRequest("Data invalida");
      

            BsonDateTime bsonDate = BsonDateTime.Create(date);

            Flight? flight = _flight.Get(IATA, RAB, bsonDate);

            if (flight == null) return NotFound();

            return JsonConvert.SerializeObject(flight, Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(FlightDTO flightDTO)
        {
            if (flightDTO == null) return NoContent();
            DateTime date;
            try
            {
                date = DateTime.Parse(flightDTO.Departure.Year + "/" + flightDTO.Departure.Month + "/" + flightDTO.Departure.Day + " 09:00");
            }
            catch
            {
                return BadRequest("Data invalida");
            }
            // Verificar se airport existe e é válido
            Airport? airport = _airport.GetValidDestiny(flightDTO.IATA).Result;
            if (airport == null) return NotFound();
            if (airport.Country != "BR") return Unauthorized();

            // Verificar se aircraft existe e é válido
            AirCraft? aircraft = _aircraft.GetAircraft(flightDTO.RAB).Result;
            if (aircraft == null) return NotFound();
            if (aircraft.Company == null) return NotFound();
            if (aircraft.Company.Status == false) return Unauthorized();

            // Verificação se data de voo é depois do último voo do aircraft
            if (aircraft.DtLastFlight != null && aircraft.DtLastFlight > date)
                return BadRequest("Data de voo não pode ser antes do último voo do avião");

            // Atualizar data de último voo de aircraft para a data do voo
            aircraft.DtLastFlight = date;
            Flight? flightaux = _flight.Get(flightDTO.IATA, flightDTO.RAB, BsonDateTime.Create(date));

            if (flightaux != null) 
                return BadRequest("voo nao pode se repetir");


            if (_aircraft.UpdateAircraft(aircraft.RAB, date) == null) return BadRequest("Impossível atualizar última data de voo do avião");

            // Inserção de flight
            Flight? flight = _flight.Insert(flightDTO, aircraft, airport, date);

            if (flight == null) return BadRequest();
            return Ok();
        }

        [HttpPatch("{IATA}, {RAB}, {departure}, {salesNumber}")]
        public ActionResult PatchSales(string IATA, string RAB, string departure, int salesNumber)
        {
            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            BsonDateTime bsonDate = BsonDateTime.Create(departureDT);

            Flight? flight = _flight.Get(IATA, RAB, bsonDate);
            if (flight == null) return NotFound();

            _flight.PatchSalesNumber(IATA, RAB, bsonDate, salesNumber);
            return Ok();
        }

        [HttpDelete("{IATA}, {RAB}, {departure}")]
        public ActionResult Delete(string IATA, string RAB, string departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();

            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            BsonDateTime bsonDate = BsonDateTime.Create(departureDT);

            Flight? flight = _flight.Delete(IATA, RAB, departureDT);

            if (flight == null) return NotFound();
            return Ok();
        }
    }
}
