using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.PassengerService.Services;

namespace OnTheFly.PassengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerServices _passengerService;
        [HttpGet]
        public ActionResult<List<Passenger>> GetAll()
        {
            return _passengerService.GetAll(); ;
        }
        [HttpGet(Name = "GetCPF")]
        public ActionResult<Passenger> GetBycpf(string cpf)
        {
            return _passengerService.GetBycpf(cpf); ;
        }
        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF is null) return NoContent();

            _passengerService.Insert(passengerdto);
            return Ok();
        }
        [HttpPut]
        public ActionResult Update(string cpf, Passenger passenger)
        {
            _passengerService.Update(cpf, passenger);
            return Ok();
        }
        [HttpDelete]
        public ActionResult Delete(string cpf)
        {
            _passengerService.Delete(cpf);
            return Ok(); ;
        }
    }
}
