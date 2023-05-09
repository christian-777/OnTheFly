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
            Passenger passenger = new()
            {
                CPF = passengerdto.CPF
            };
            if (passenger.ValidateCPF(cpf))
            {
                //int number = passengerdto.Number;

                Address? address = _postOfficeService.GetAddress(passengerdto.Zipcode).Result;
                if (address is null)
                    return BadRequest("Endereço inexistente!");

                //passengerdto.Number = number;

                if (address.Street == "")
                {
                    if (passengerdto.Street == "")
                        return NotFound("Rua não obtida! Informar manualmente.");
                    else
                        passenger.Address.Street = address.Street;
                }
                else
                {
                    if (!address.Street.Equals(passengerdto.Street))
                        passenger.Address.Street = address.Street;

                }
                passenger.Address.Street = passengerdto.Street;
                passenger.Address.City = address.City;
                passenger.Address.Complement = address.Complement;
                passenger.Address.Number = passengerdto.Number;
                passenger.Address.State = address.State;
                passenger.Address.Zipcode = address.Zipcode;
                passenger.CPF = passengerdto.CPF;
                passenger.DtBirth = passengerdto.DtBirth;
                passenger.DtRegister = passengerdto.DtRegister;
                passenger.Gender = passengerdto.Gender;
                passenger.Name = passengerdto.Name;
                passenger.Phone = passengerdto.Phone;
                passenger.Status = passengerdto.Status;

                _passengerConnection.Insert(passenger);
                return Ok("Inserido com sucesso!");
            }
            return BadRequest("Erro ao inserir CPF!");

        }

        [HttpPut]
        public ActionResult Update(string cpf, PassengerForPut passengerput)
        {
            if (cpf is null) return NotFound("CPF não informado!");

            var passenger = _passengerConnection.FindPassenger(cpf);

            if (passengerput.Name != "string")
            {
                passenger.Name = passengerput.Name;
            }
            if (passengerput.Gender != "string")
            {
                passenger.Gender = passengerput.Gender;
            }
            if (passengerput.DtBirth != passenger.DtBirth)
            {
                passenger.DtRegister = passenger.DtRegister;
            }
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
