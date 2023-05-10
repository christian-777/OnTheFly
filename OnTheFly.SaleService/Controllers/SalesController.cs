using System.Net.Sockets;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
        private readonly FlightConnection _flight;
        private readonly PassengerService _passenger;
        private readonly ConnectionFactory _factory;
        public SalesController(SaleConnection saleConnection, FlightConnection flight, PassengerService passenger, ConnectionFactory factory)
        {
            _saleConnection = saleConnection;
            _flight = flight;
            _passenger = passenger;
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
        public ActionResult<string> GetSale(string CPF, string IATA, string RAB, string departure)
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

            Sale? sale = _saleConnection.FindSale(CPF, IATA, RAB, date);
            if (sale == null)
                return NotFound("Venda não encontrada");

            return JsonConvert.SerializeObject(sale, Newtonsoft.Json.Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(SaleDTO saleDTO)
        {
            if (saleDTO.Passengers == null) return BadRequest("O número de passageiros está nulo");

            if (saleDTO.Reserved == saleDTO.Sold)
                return BadRequest("Status de venda ou agendamento invalido");

            string rab = saleDTO.RAB.Replace("-", "");
            if (rab.Length != 5)
                return BadRequest("Quantidade de caracteres de RAB inválida");

            if (!AirCraft.RABValidation(rab))
                return BadRequest("RAB inválido");

            DateTime date;
            try
            {
                date = DateTime.Parse(saleDTO.Departure.Year + "/" + saleDTO.Departure.Month + "/" + saleDTO.Departure.Day+" 09:00");
            }
            catch
            {
                return BadRequest("Data invalida");
            }

            Flight? flight = _flight.Get(saleDTO.IATA, rab, BsonDateTime.Create(date));
            if (flight == null) return NotFound("Voo não encontrado");

            List<string> passengers = new List<string>();

            foreach (string cpf in saleDTO.Passengers)
            {
                Passenger? passenger = _passenger.GetPassenger(cpf).Result;
                if (passenger == null) return NotFound("Passageiro não encontrado");
                if (!passenger.Status) return BadRequest("Existem passageiros impedidos de comprar");
                if (Passenger.ValidateAge(passenger) < 18 && passengers.Count == 0) return Unauthorized("Menores de idade não podem ser o cadastrante da venda");

                passengers.Add(passenger.CPF);
            }

            foreach (var passenger in passengers)
            {
                var elements=passengers.FindAll(p=>p==passenger);
                if (elements.Count != 1)
                    return BadRequest("Não é permitida a compra de mais de uma passagem por passageiro");
            }

            if (passengers.Count + flight.Sales > flight.Plane.Capacity)
                return BadRequest("A quantidade de passagens excede a capacidade do avião");

            _flight.UpdateSales(flight.Destiny.IATA, flight.Plane.RAB, flight.Departure, passengers.Count);
               

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
            }
            return Ok("Venda enviada ao banco com sucesso");
        }

        [HttpPut("/UpdateSale/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult UpdateSale(string CPF, string IATA, string RAB, string departure)
        
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

            Sale? sale = _saleConnection.FindSale(CPF, IATA, RAB, date);
            if (sale == null) return NotFound("Venda não encontrada");

            if (_saleConnection.Update(CPF, IATA, RAB, date, sale))
                return Ok("Status atualizado com sucesso");
            else
                return BadRequest("Falha ao atualizar status");
        }

        [HttpPost("/SendToDeleted/{CPF},{IATA},{RAB},{departure}")]
        public ActionResult Delete(string CPF, string IATA, string RAB, string departure)
        {
            var data = departure.Split('-');
            DateTime date;
            try
            {
                date = DateTime.Parse(data[0] + "/" + data[1] + "/" + data[2] + " 9:00");
            }
            catch
            {
                return BadRequest("Data invalida");
            }

            Sale? sale = _saleConnection.FindSale(CPF, IATA, RAB, date);
            if (sale == null) return NotFound("Venda não encontrada");

            _saleConnection.Delete(CPF, IATA, RAB, date);
            return Ok("Deletado com sucesso"); ;
        }
    }
}
