using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        
        [HttpGet]
        public ActionResult<string> GetAll()
        {
            var sales = _saleConnection.FindAll();
            if(sales == null)
                return NoContent();

            return JsonConvert.SerializeObject(sales, Newtonsoft.Json.Formatting.Indented);
        }
        [HttpGet("/getsale/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult<string> GetSale(string CPF, string IATA, string RAB, DateTime departure)
        {
            var sales = _saleConnection.FindSale(CPF, IATA, RAB, departure);
            if (sales == null)
                return NoContent();
            return JsonConvert.SerializeObject(sales, Newtonsoft.Json.Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(SaleDTO saleDTO)
        {
            if (saleDTO.Reserved == saleDTO.Sold)
                return BadRequest("staus de venda ou agendamento invalido");

            Flight? flight = _flight.GetFlight(saleDTO.IATA, saleDTO.RAB, saleDTO.Departure).Result;
            if (flight == null) return NotFound();

            List<Passenger> passengers = new List<Passenger>();
            foreach (string cpf in saleDTO.Passengers)
            {
                Passenger? passenger = _passenger.GetPassenger(cpf).Result;
                if (passenger == null) return NotFound();
                if (passenger.Status) return BadRequest("passageiro impedido de comprar");
                passengers.Add(passenger);
            }
            //calculo de idade
            //if (passengers[0].)
              //  return BadRequest("menores nao sao permitidos comprar passagens");

            foreach(var passenger in passengers)
            {
                var elements=passengers.FindAll(p=>p==passenger);
                if (elements.Count != 1)
                    return BadRequest("Nao eprmitida a copra de mais de 1 passagem por passageiro");
            }

            if (passengers.Count + flight.Sales > flight.Plane.Capacity)
                return BadRequest("a quantidade de passagens excede a capacidade do aviao, por favor tente em outro voo");

            //precisa do metodo de put de flight pra mudar a quantdade de vendas

            // Não se pode ter o mesmo cpf em vários passageiros - CONCLUIDO
            // O primeiro da lista não pode ser maior de idade - PRECISA A PARTE DE CALCULO DE IDADE DE PASSENGER
            // Não se pode ter passageiros restritos - COMO A COLLECTION DE RESTRICTED É SEPARADA E ACTIVATE NUNCA VAMOS LIDAR COM OS RESTRITOS

            _saleConnection.Insert(saleDTO, flight, passengers);
            return Ok();
        }
        [HttpPut("/sell/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult Update(string CPF, string IATA, string RAB, DateTime departure)
        {
            var sale = _saleConnection.FindSale(CPF, IATA, RAB, departure);
            if (sale == null) return NotFound();
            if (sale.Reserved)
            {
                sale.Reserved=!sale.Reserved;
                sale.Sold=!sale.Sold;
            }
            if (_saleConnection.Update(CPF, IATA, RAB, departure, sale))
                return Ok();
            else
                return BadRequest("Falha ao atualizar status");
        }
        [HttpDelete("/delete/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult Delete(string CPF, string IATA, string RAB, DateTime departure)
        {
            _saleConnection.Delete(CPF, IATA, RAB, departure);
            return Ok(); ;
        }
    }
}
