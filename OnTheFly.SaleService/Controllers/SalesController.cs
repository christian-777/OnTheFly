using System.Net.Sockets;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.SaleService.Services;
using RabbitMQ.Client;

namespace OnTheFly.SaleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SaleConnection _saleConnection;
        private readonly FlightService _flight;
        private readonly PassengerService _passenger;
        private readonly FlightConnection _flightService;
        private readonly ConnectionFactory _factory;
        public SalesController(SaleConnection saleConnection, FlightService flight, PassengerService passenger, ConnectionFactory factory, FlightConnection flightconn)
        {
            _saleConnection = saleConnection;
            _flight = flight;
            _passenger = passenger;
            _flightService = flightconn;
            _factory = factory;
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
            /*if (saleDTO.Reserved == saleDTO.Sold)
                return BadRequest("Status de venda ou agendamento invalido");

            Flight? flight = _flightService.Get(saleDTO.IATA, saleDTO.RAB, saleDTO.Departure);
            if (flight == null) return NotFound();

            List<Passenger> passengers = new List<Passenger>();
            foreach (string cpf in saleDTO.Passengers)
            {
                Passenger? passenger = _passenger.GetPassenger(cpf).Result;
                if (passenger == null) return NotFound();
                if (!passenger.Status) return BadRequest("Existem passageiros impedidos de comprar");
                passengers.Add(passenger);
            }
         
            if (passengers[0].ValidateAge())
                return BadRequest("Menores nao sao permitidos comprar passagens");

            foreach (var passenger in passengers)
            {
                var elements=passengers.FindAll(p=>p==passenger);
                if (elements.Count != 1)
                    return BadRequest("Não é permitida a compra de mais de uma passagem por passageiro");
            }

            if (passengers.Count + flight.Sales > flight.Plane.Capacity)
                return BadRequest("A quantidade de passagens excede a capacidade do avião"); 

            if (_flight.PatchFlight(flight.Destiny.IATA, flight.Plane.RAB, flight.Departure, passengers.Count) == null)
                return BadRequest("nao foi possivel atualizar o voo");

            Sale sale = new Sale
            {
                Flight = flight,
                Passengers = passengers,
                Reserved = saleDTO.Reserved,
                Sold = saleDTO.Sold
            };

            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    channel.QueueDeclare(
                        queue: "Sales",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    channel.QueueDeclare(
                        queue: "Reservation",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var stringfieldMessage = JsonConvert.SerializeObject(sale);
                    var bytesMessage = Encoding.UTF8.GetBytes(stringfieldMessage);

                    string queue;
                    if (sale.Reserved)
                    {
                        queue = "Reservation";
                    }
                    else
                    {
                        queue = "Sales";
                    }

                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queue,
                        basicProperties: null,
                        body: bytesMessage
                        );
                }
            }*/
            return Accepted();

            //_saleConnection.Insert(sale);
            return Ok();
        }

        [HttpPatch("/sell/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult PatchSaleStatus(string CPF, string IATA, string RAB, DateTime departure)
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
