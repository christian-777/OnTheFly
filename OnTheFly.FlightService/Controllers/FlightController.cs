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

        [HttpGet]
        public ActionResult<List<Flight>> GetAll()
        {
            List<Flight> flights = _flight.FindAll();

            if (flights.Count == 0)
                return NotFound("Nenhum avião encontrado");

            return flights;
        }

        [HttpGet("{IATA},{RAB},{departure}")]
        public ActionResult<string> Get(string IATA, string RAB, string departure)
        {
            var data = departure.Split('-');
            DateTime date;
            try
            {
                date = DateTime.Parse(data[0] + "/" + data[1] + "/" + data[2] + " 09:00");
            }
            catch
            {
                return BadRequest("Data invalida");
            }

            BsonDateTime bsonDate = BsonDateTime.Create(date);

            Flight? flight = _flight.Get(IATA, RAB, bsonDate);

            if (flight == null) return NotFound("Voo não encontrado");

            return JsonConvert.SerializeObject(flight, Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(FlightDTO flightDTO)
        {
            if (flightDTO == null) return BadRequest("Nenhum voo inserido");
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
            if (airport == null) return NotFound("Aeroporto não encotrado");
            if (airport.Country == null || airport.Country == "") NotFound("País de origem do aeroporto não encontrado");
            if (airport.Country != "BR") return Unauthorized("Não são autorizados voos fora do Brasil");

            // Verificar se aircraft existe e é válido
            AirCraft? aircraft = _aircraft.GetAircraft(flightDTO.RAB).Result;
            if (aircraft == null) return NotFound("Avião não encontrado");
            if (aircraft.Company == null) return NotFound("Companhia de avião não encontrada");
            if (aircraft.Company.Status == false) return Unauthorized("Companhia não autorizada para voos");

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

            if (flight == null) return BadRequest("Não foi possivel enviar voo para o banco");
            return Ok("Voo armazenado no banco com sucesso!");
        }

        [HttpPut("/UpdateStatus/{IATA}, {RAB}, {departure}")]
        public ActionResult UpdateStatus(string IATA, string RAB, string departure)
        {
            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            if (departureDT.Hour != 12)
                departureDT = departureDT.AddHours(9);

            BsonDateTime bsonDate = BsonDateTime.Create(departureDT);

            Flight? flight = _flight.Get(IATA, RAB, bsonDate);
            if (flight == null) return NotFound("Voo não encontrado");

            if (!_flight.UpdateStatus(IATA, RAB, bsonDate))
                return BadRequest("Não foi possível atualizar o status do voo");

            return Ok("Voo atualizado com sucesso!");
        }

        [HttpPut("/UpdateSales/{IATA}, {RAB}, {departure}, {salesNumber}")]
        public ActionResult UpdateSales(string IATA, string RAB, string departure, int salesNumber)
        {
            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            if (departureDT.Hour != 12)
                departureDT = departureDT.AddHours(9);

            BsonDateTime bsonDate = BsonDateTime.Create(departureDT);

            Flight? flight = _flight.Get(IATA, RAB, bsonDate);
            if (flight == null) return NotFound("Voo não encontrado");

            if (!_flight.UpdateSales(IATA, RAB, bsonDate, salesNumber))
                return BadRequest("Não foi possível atualizar o número de vendas do voo");

            return Ok("Voo atualizado com sucesso!");
        }

        [HttpPost("/SendToDeleted/{IATA}, {RAB}, {departure}")]
        public ActionResult Delete(string IATA, string RAB, string departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();

            bool isDate = DateTime.TryParse(departure, out DateTime departureDT);
            if (!isDate) return BadRequest("Formato de data não reconhecido");

            if (departureDT.Hour != 12)
                departureDT = departureDT.AddHours(9);

            BsonDateTime bsonDate = BsonDateTime.Create(departureDT);

            if (!_flight.Delete(IATA, RAB, departureDT))
                return BadRequest("Não foi possível deletar o voo");

            return Ok("Voo deletado com sucesso!");
        }
    }
}
