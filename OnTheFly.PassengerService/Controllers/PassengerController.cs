using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;


namespace OnTheFly.PassengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerConnection _passengerConnection;
        [HttpGet]
        public ActionResult<string> GetAll()
        {
            return JsonConvert.SerializeObject(_passengerConnection.FindAll(), Formatting.Indented);
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
