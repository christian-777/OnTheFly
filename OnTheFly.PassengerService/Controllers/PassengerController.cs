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
        public ActionResult<string> GetAll()
        {
            if (_passengerConnection.FindAll().Count == 0)
                return Ok("Lista sem CPF");
            return JsonConvert.SerializeObject(_passengerConnection.FindAll(), Formatting.Indented);
        }
        [HttpGet("{CPF}", Name = "GetCPF")]
        public ActionResult<string> GetBycpf(string CPF)
        {
            if (CPF is null) return NotFound("CPF não informado!");
            if (_passengerConnection.FindPassengerRestrict(CPF) is not null)
                return BadRequest("CPF restrito!");
            return JsonConvert.SerializeObject(_passengerConnection.FindPassenger(CPF), Formatting.Indented);
        }
        [HttpGet("/Restrict/{CPF}", Name = "GetCPFRestrict")]
        public ActionResult<string> GetBycpfRestrict(string CPF)
        {
            if (CPF is null) return NotFound("CPF não informado!");
  
            return JsonConvert.SerializeObject(_passengerConnection.FindPassengerRestrict(CPF.Replace(".", "").Replace("-", "")), Formatting.Indented);
        }
        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF == null) return NotFound("CPF não informado!");

            string cpf = passengerdto.CPF.Replace(".", "").Replace("-", "");
            if (_passengerConnection.FindPassenger(cpf) is not null)
                return BadRequest("CPF já cadastrado!");

            if (!long.TryParse(cpf, out var aux))
                return BadRequest("CPF Inválido!");
            Passenger passenger = new()
            {
                CPF = passengerdto.CPF
            };

            if (_passengerConnection.FindPassengerRestrict(cpf) is not null)
                return BadRequest("CPF restrito!");

            if (passenger.ValidateCPF(cpf))
            {
                Address? address = _postOfficeService.GetAddress(passengerdto.Zipcode).Result;
                if (address == null)
                    return BadRequest("Endereço inexistente!");

                passenger.Address = new();
                if (address.Street == "" || address.Street is null)
                {
                    if (passengerdto.Street == "string" || passengerdto.Street is null)
                        return NotFound("Rua não obtida! Informar manualmente.");
                    else
                        passenger.Address.Street = passengerdto.Street;
                }
                else passenger.Address.Street = address.Street;

                passenger.Address.City = address.City;
                passenger.Address.Complement = address.Complement;
                passenger.Address.Number = passengerdto.Number;
                passenger.Address.State = address.State;
                passenger.Address.Zipcode = address.Zipcode;
                passenger.CPF = cpf;
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
        [HttpPost("/Restrict/{CPF}", Name = "InsertRestrict")]
        public ActionResult InsertRestrict(string CPF)
        {
            var cpfrestrict = _passengerConnection.FindPassengerRestrict(CPF.Replace(".", "").Replace("-", ""));
            _passengerConnection.Insert(cpfrestrict);
            _passengerConnection.DeleteFull(cpfrestrict.CPF);
            return Ok();
        }
        [HttpPut]
        public ActionResult Update(string cpf, PassengerForPut passengerput)
        {
            if (cpf is null) return NotFound("CPF não informado!");

            if (_passengerConnection.FindPassengerRestrict(cpf) is not null)
                return BadRequest("CPF restrito!");

            Passenger? passenger = _passengerConnection.FindPassenger(cpf);
            if (passenger == null) return NotFound();

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
        //[HttpPut("/Address/{CPF}, {Address}", Name = "UpdateAddress")]
        //public ActionResult UpdateAddress(string CPF, Address address)
        //{
        //    var cpf = CPF.Replace(".", "").Replace("-", "");
        //    if (_passengerConnection.FindPassengerRestrict(cpf) is not null)
        //        return BadRequest("CPF restrito!");
        //    _passengerConnection.FindPassenger(cpf);
        //    return Ok();
        //}
        //[HttpPatch]
        //public ActionResult
        [HttpDelete("{CPF}")]
        public ActionResult Delete(string CPF)
        {
            if (CPF is null) return NotFound("CPF não informado!");
            if (_passengerConnection.FindPassenger(CPF) is null && _passengerConnection.FindPassengerRestrict(CPF) is null)
                return BadRequest("CPF não encontrado no banco!");

            _passengerConnection.Delete(CPF.Replace(".", "").Replace("-", ""));
            return Ok("CPF deletado com sucesso!");
        }
        [HttpDelete("/Restrict/{CPF}", Name = "Restrict")]
        public ActionResult Restrict(string CPF)
        {
            if (CPF is null) return NotFound("CPF não informado!");

            if (_passengerConnection.FindPassenger(CPF) is null)
                return BadRequest("CPF não encontrado no banco!");

            _passengerConnection.Restrict(CPF);
            return Ok("CPF restringido com sucesso!"); ;
        }
    }
}
