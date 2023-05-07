using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.SaleService.Services;

namespace OnTheFly.SaleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SaleConnection _saleConnection;
        private readonly FlightService _flight;
        private readonly PassengerService _passenger;

        public SalesController(SaleConnection saleConnection, FlightService flight, PassengerService passenger)
        {
            _saleConnection = saleConnection;
            _flight = flight;
            _passenger = passenger;
        }

        /*
        [HttpGet]
        public ActionResult<string> GetAll()
        {
            var sales = _saleConnection.FindAll();
            if(sales == null)
                return NoContent();

            return JsonConvert.SerializeObject(sales, Formatting.Indented);
        }
        [HttpGet("{cpf}", Name = "GetCPF")]
        public ActionResult<string> GetBycpf(string cpf)
        {
            return JsonConvert.SerializeObject(_passengerConnection.FindPassenger(cpf), Formatting.Indented);
        }
        */

        [HttpPost]
        public ActionResult Insert(SaleDTO saleDTO)
        {
            Flight? flight = _flight.GetFlight(saleDTO.IATA, saleDTO.RAB, saleDTO.Departure).Result;
            if (flight == null) return NotFound();

            List<Passenger> passengers = new List<Passenger>();
            foreach (string cpf in saleDTO.Passengers)
            {
                Passenger? passenger = _passenger.GetPassenger(cpf).Result;
                if (passenger == null) return NotFound();
                passengers.Add(passenger);
            }

            // Não se pode ter o mesmo cpf em vários passageiros
            // O primeiro da lista não pode ser maior de idade
            // Não se pode ter passageiros restritos

            _saleConnection.Insert(saleDTO, flight, passengers);
            return Ok();
        }
        [HttpPut]
        public ActionResult Update(string cpf, Passenger passenger)
        {
            _passengerConnection.Update(cpf, passenger);
            return Ok();
        }
        [HttpDelete]
        public ActionResult Delete(string cpf)
        {
            _passengerConnection.Delete(cpf);
            return Ok(); ;
        }
    }
}
