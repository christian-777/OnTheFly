
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.Services;

namespace OnTheFly.PassengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerConnection _passengerConnection;
        private readonly PostOfficeService _postOfficeService;

        public PassengerController(PassengerConnection passengerConnection, PostOfficeService postOfficeService)
        {
            _passengerConnection = passengerConnection;
            _postOfficeService = postOfficeService;
        }
        [HttpGet]
        public ActionResult<string> GetAll()
        {
            return JsonConvert.SerializeObject(_passengerConnection.FindAll(), Formatting.Indented);
        }
        [HttpGet("{cpf}", Name = "GetCPF")]
        public ActionResult<string> GetBycpf(string cpf)
        {
            if (cpf is null) return NotFound("CPF não informado!");

            return JsonConvert.SerializeObject(_passengerConnection.FindPassenger(cpf), Formatting.Indented);
        }
        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF is null) return NotFound("CPF não informado!");
            
            var cpf = passengerdto.CPF.Replace(".", "").Replace("-", "");

            if (!long.TryParse(cpf, out var aux))
                return BadRequest("CPF Inválido!");

            if (passengerdto.ValidateCPF(cpf))
            {
                int number = passengerdto.Address.Number;

                Address address = _postOfficeService.GetAddress(passengerdto.Address.Zipcode).Result;

                address.Number = number;
                passengerdto.Address = address;
                if (passengerdto.Address.Street == "")
                {
                    return NotFound("Rua não obtida! Informar manualmente.");
                }
                _passengerConnection.Insert(passengerdto);
                return Ok("Inserido com sucesso!");
            }
            return BadRequest("Erro ao inserir CPF!");

        }

        [HttpPut]
        public ActionResult Update(string cpf, PassengerForPut passengerput)
        {
            if (cpf is null) return NotFound("CPF não informado!");

            var passenger = _passengerConnection.FindPassenger(cpf);

            passenger.CPF = cpf;
            passenger.Name = passengerput.Name;
            passenger.Gender = passengerput.Gender;
            passenger.DtRegister = passenger.DtRegister;
            passenger.DtBirth = passengerput.DtBirth;
            passenger.Phone = passengerput.Phone;
            passenger.Address = passengerput.Address;

            _passengerConnection.Update(cpf, passenger);
            return Ok();
        }
        [HttpDelete]
        public ActionResult Delete(string cpf)
        {
            if (cpf is null) return NotFound("CPF não informado!");

            _passengerConnection.Delete(cpf);
            return Ok(); ;
        }
    }
}
