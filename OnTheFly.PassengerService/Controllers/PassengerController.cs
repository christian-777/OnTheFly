
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.PostOfficeService;

namespace OnTheFly.PassengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerConnection _passengerConnection;
        private readonly PostOfficesService _postOfficeService;

        public PassengerController(PassengerConnection passengerConnection, PostOfficesService postOfficeService)
        {
            _passengerConnection = passengerConnection;
            _postOfficeService = postOfficeService;
        }
       
        [HttpGet]
        public ActionResult<List<Passenger>> GetAll()
        {
            var passengers = _passengerConnection.FindAll();
            if (passengers == null)
                return NotFound("Nenhum passageiro encontrado");
            return passengers;
        }

        [HttpGet("{cpf}", Name = "GetCPF")]
        public ActionResult<Passenger> GetBycpf(string cpf)
        {
            if (cpf is null || cpf.Equals("string") || cpf=="") 
                return BadRequest("CPF não informado!");

            var passenger= _passengerConnection.FindPassenger(cpf);

            if (passenger == null)
                return NotFound("Passageiro com este cpf nao encontrado");

            _passengerConnection.FindPassengerRestrict(cpf);

            if (_passengerConnection.FindPassengerRestrict(cpf) == null)
                return BadRequest("CPF restrito!");

            return passenger;
        }

        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF == null || passengerdto.CPF.Equals("string") || passengerdto.CPF=="") 
                return BadRequest("CPF não informado!");

            string cpf = passengerdto.CPF.Replace(".", "").Replace("-", "");

            if (!long.TryParse(cpf, out var aux))
                return BadRequest("CPF Inválido!");

            if (!Passenger.ValidateCPF(cpf))
                return BadRequest("CPF invalido");

            passengerdto.Zipcode = passengerdto.Zipcode.Replace("-", "");
            var auxAddress = _postOfficeService.GetAddress(passengerdto.Zipcode).Result;
            if (auxAddress == null)
                return NotFound("Endereço nao encontrado");

            Address address = new()
            {
                Number = passengerdto.Number,
                City=auxAddress.City,
                Complement=auxAddress.Complement,
                State = auxAddress.State,
                Zipcode=passengerdto.Zipcode
            };

            if (auxAddress.Street != "")
                address.Street = auxAddress.Street;
            else
            {
                if (passengerdto.Street != "" || passengerdto.Street.Equals("string") || passengerdto.Street != null)
                    address.Street = passengerdto.Street;
                else
                    return BadRequest("O campo Street é obrigatorio");
            }


            Passenger passenger = new()
            {
                CPF = cpf,
                Address = address,
                DtBirth = DateTime.Parse(passengerdto.DtBirth.Year + "/" + passengerdto.DtBirth.Month + "/" + passengerdto.DtBirth.Day),
                DtRegister=DateTime.Now,
                Gender=passengerdto.Gender,
                Name = passengerdto.Name,
                Phone=passengerdto.Phone,
                Status=passengerdto.Status
            };

             if(_passengerConnection.Insert(passenger)!=null)
                return Ok("Inserido com sucesso!");
            
            return BadRequest("Erro ao inserir Passageiro!");

        }

        [HttpDelete("{cpf}")]
        public ActionResult Delete(string cpf)
        {
            if (cpf == null) return BadRequest("CPF não informado!");

            _passengerConnection.Delete(cpf);
            return Ok(); ;
        }
    }
}
