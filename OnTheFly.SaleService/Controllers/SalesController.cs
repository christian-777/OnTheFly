using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnTheFly.Connections;

namespace OnTheFly.SaleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly SaleConnection _saleConnection;
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
        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF is null) return NoContent();

            _passengerConnection.Insert(passengerdto);
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
