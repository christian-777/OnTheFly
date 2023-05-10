using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.IdGenerators;
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
            if (passengers.Count==0)
                return NotFound("Nenhum passageiro encontrado");
            return passengers;
        }

        [HttpGet("{CPF}", Name = "GetCPF")]
        public ActionResult<Passenger> GetBycpf(string CPF)
        {
            if (CPF is null || CPF.Equals("string") || CPF=="") 
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");
            if (Passenger.ValidateCPF(CPF) == false)
                return BadRequest("CPF invalido");

            var passenger= _passengerConnection.FindPassenger(CPF);

            if (passenger == null)
                return NotFound("Passageiro com este cpf nao encontrado");

            return passenger;
        }

        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF == null || passengerdto.CPF.Equals("string") || passengerdto.CPF=="") 
                return BadRequest("CPF não informado!");

            var cpf = passengerdto.CPF.Replace(".", "").Replace("-", "");

            if (Passenger.ValidateCPF(cpf)==false)
                return BadRequest("CPF invalido");

            if (_passengerConnection.FindPassengerRestrict(passengerdto.CPF) != null)
                return BadRequest("Passageiro restrito!!");

            if (_passengerConnection.FindPassengerDeleted(passengerdto.CPF) != null)
                return BadRequest("Impossivel inserir este passageiro");

            if (_passengerConnection.FindPassenger(passengerdto.CPF) != null)
                return Conflict("Passageiro ja cadastrado");

            DateTime date;
            try
            {
                date = DateTime.Parse(passengerdto.DtBirth.Year + "/" + passengerdto.DtBirth.Month + "/" + passengerdto.DtBirth.Day);
            }
            catch
            {
                return BadRequest("Data invalida");
            }
            if(DateTime.Now.Subtract(date).TotalDays < 0)
                return BadRequest("Data invalida");

            passengerdto.Zipcode = passengerdto.Zipcode.Replace("-", "");
            var auxAddress = _postOfficeService.GetAddress(passengerdto.Zipcode).Result;
            if (auxAddress == null)
                return NotFound("Endereço nao encontrado");

            if (passengerdto.Number == 0)
                return BadRequest("Campo Number é obrigatorio");

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
                DtBirth = date,
                DtRegister=DateTime.Now,
                Gender=passengerdto.Gender,
                Name = passengerdto.Name,
                Phone=passengerdto.Phone,
                Status=passengerdto.Status
            };

            var insertPassenger = _passengerConnection.Insert(passenger);
             if (insertPassenger!=null)
                return Created("","Inserido com sucesso!\n\n"+JsonConvert.SerializeObject(insertPassenger, Formatting.Indented));
            
            return BadRequest("Erro ao inserir Passageiro!");

        }

        [HttpPost("/SendToDeleted/{CPF}")]
        public ActionResult Delete(string CPF)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "") 
                return BadRequest("CPF não informado!");

            CPF =CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (_passengerConnection.FindPassenger(CPF) != null || _passengerConnection.FindPassengerRestrict(CPF) != null)
            {
                if (_passengerConnection.Delete(CPF))
                    return Ok("Passageiro deletado com sucesso!");
                else
                    return BadRequest("erro ao deletar");
            }
            return BadRequest("passageiro inexistente");
        }

        [HttpPost("/SendToRestricted/{CPF}")]
        public ActionResult Restrict(string CPF)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (_passengerConnection.FindPassenger(CPF) != null)
            {
                if (_passengerConnection.Restrict(CPF))
                    return Ok("Passageiro restrito com sucesso!");
                else
                    return BadRequest("erro ao restringir");
            }
            return BadRequest("passageiro inexistente");
        }

        [HttpPost("/UnrestrictPassenger/{CPF}")]
        public ActionResult Unrestrict(string CPF)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (_passengerConnection.FindPassengerRestrict(CPF) != null)
            {
                if (_passengerConnection.Unrestrict(CPF))
                    return Ok("Passageiro retirado da lista de restritos com sucesso!");
                else
                    return BadRequest("erro ao retirar da lista de restritos");
            }
            return BadRequest("passageiro nao esta na lista de restritos");
        }

        [HttpPost("/UndeletPassenger/{CPF}")]
        public ActionResult UndeletePassenger(string CPF)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (_passengerConnection.FindPassengerDeleted(CPF) != null)
            {
                if (_passengerConnection.UndeletPassenger(CPF))
                    return Ok("Passageiro retirado da lista de deletados com sucesso!");
                else
                    return BadRequest("erro ao retirar da lista de deletados");
            }
            return BadRequest("passageiro nao esta na lista de deletados");
        }

        [HttpPut("/UpdateName/{CPF},{Name}")]
        public ActionResult UpdateName(string CPF, string Name)
        {
            if(CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.Name = Name;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Nome do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o nome do Passageiro");
            }
            
            return BadRequest("passageiro nao esta na lista");
        }

        [HttpPut("/UpdateGender/{CPF},{Gender}")]
        public ActionResult UpdateGender(string CPF, string Gender)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (Gender.Length != 1)
                return BadRequest("O campo genero aceita apenas um caractere");

            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.Gender = Gender;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Genero do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o genero do Passageiro");
            }

            return BadRequest("passageiro nao esta na lista");
        }

        [HttpPut("/UpdatePhone/{CPF},{Phone}")]
        public ActionResult UpdatePhone(string CPF, string Phone)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            if (Phone.Length > 14)
                return BadRequest("Digite um telefone valido");

            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.Phone = Phone;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Telefone do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o telefone do Passageiro");
            }

            return BadRequest("passageiro nao esta na lista");
        }

        [HttpPut("/UpdateDtBirth/{CPF}")]
        public ActionResult UpdateDtBirth(string CPF, [FromBody] DateDTO DtBirth)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            DateTime date;
            try
            {
                date = DateTime.Parse(DtBirth.Year + "/" + DtBirth.Month + "/" + DtBirth.Day);
            }
            catch
            {
                return BadRequest("Data invalida");
            }
            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.DtBirth = date;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Data de nascimento do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar a data de nascimento do Passageiro");
            }

            return BadRequest("passageiro nao esta na lista");
        }

        [HttpPut("/UpdateAddress/{CPF}")]
        public ActionResult UpdateAddress(string CPF, Address address)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            address.Zipcode = address.Zipcode.Replace("-", "");

            var auxAddress = _postOfficeService.GetAddress(address.Zipcode).Result;
            if (auxAddress == null)
                return NotFound("Endereço nao encontrado");

            if (address.Number == 0)
                return BadRequest("Campo Number é obrigatorio");

            address.City = auxAddress.City;
            address.Complement = auxAddress.Complement;
            address.State = auxAddress.State;

            if (auxAddress.Street != "")
                address.Street = auxAddress.Street;
            else
            {
                if (address.Street == "" || address.Street.Equals("string") || address.Street == null)
                    return BadRequest("O campo Street é obrigatorio");
            }

            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.Address = address;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Endereço do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o endereço do Passageiro");
            }

            return BadRequest("passageiro nao esta na lista");
        }

        [HttpPut("/ChangeStatus/{CPF}")]
        public ActionResult ChangeStatus(string CPF)
        {
            if (CPF == null || CPF.Equals("string") || CPF == "")
                return BadRequest("CPF não informado!");

            CPF = CPF.Replace(".", "").Replace("-", "");

            if (!Passenger.ValidateCPF(CPF))
                return BadRequest("CPF invalido");

            
            var passenger = _passengerConnection.FindPassenger(CPF);
            if (passenger != null)
            {
                passenger.Status = !passenger.Status;
                if (_passengerConnection.Update(CPF, passenger))
                    return Ok("Status do Passageiro atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o status do Passageiro");
            }

            return BadRequest("passageiro nao esta na lista");
        }
    }
}
