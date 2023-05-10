using System.Collections.Generic;
using DocumentValidator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.CompanyService.Services;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.PostOfficeService;
using System.IO;

namespace OnTheFly.CompanyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyConnection _companyConnection;
        private readonly PostOfficesService _postOfficeService;
        private readonly AircraftService _aircraftService;

        public CompaniesController(CompanyConnection companyConnection, PostOfficesService postOfficeService, AircraftService aircraftService)
        {
            _companyConnection = companyConnection;
            _postOfficeService = postOfficeService;
            _aircraftService = aircraftService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Company>>> GetCompany()
        {
            if (_companyConnection.FindAll().Count == 0)
            {
                return NotFound("Nenhuma companhia cadastrada");
            }
            return _companyConnection.FindAll();
        }

        [HttpGet("{cnpj}")]
        public async Task<ActionResult<Company>> GetCompanyByCNPJ(string cnpj)
        {
            cnpj = cnpj.Replace("%2F", "").Replace(".", "").Replace("-", "").Replace("/", "");
            if (!CnpjValidation.Validate(cnpj))
                return BadRequest("Cnpj Invalido");

            if (_companyConnection.FindByCnpj(cnpj) == null)
                return NotFound("O CNPJ informado nao encontrado");

            return _companyConnection.FindByCnpj(cnpj);
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostCompany(CompanyDTO companyDTO)
        {
            #region Company
            companyDTO.Cnpj = companyDTO.Cnpj.Replace("%2F", "").Replace(".", "").Replace("-", "").Replace("/", "");
            if (companyDTO.Cnpj == null) return BadRequest("Cnpj não encontrado");
            if (!CnpjValidation.Validate(companyDTO.Cnpj))
                return BadRequest("Cnpj Invalido");

            if ((companyDTO.Name == "") || (companyDTO.Name == "string"))
                return BadRequest("A razão social da companhia não foi informada! Por favor, insira um nome correspondente a razão social da companhia");

            if ((companyDTO.NameOPT == "") || (companyDTO.NameOPT == "string"))
                companyDTO.NameOPT = companyDTO.Name;
            #endregion

            #region Date
            DateTime date;
            try
            {
                date = DateTime.Parse(companyDTO.DtOpen.Year + "/" + companyDTO.DtOpen.Month + "/" + companyDTO.DtOpen.Day);
            }
            catch
            {
                return BadRequest("A data informada é inválida! Por favor, informe uma data de abertura da companhia válida");
            }
            if (DateTime.Now.Subtract(date).TotalDays < 0)
                return BadRequest("A data informada é inválida! Por favor, informe uma data de abertura da companhia válida");
            #endregion

            #region Address
            companyDTO.Zipcode = companyDTO.Zipcode.Replace("-", "");
            var auxAddress = _postOfficeService.GetAddress(companyDTO.Zipcode).Result;
            if (auxAddress == null)
                return NotFound("Endereço nao encontrado");

            if (companyDTO.Number == 0)
                return BadRequest("Campo Number é obrigatorio");

            Address address = new()
            {
                Number = companyDTO.Number,
                City = auxAddress.City,
                Complement = auxAddress.Complement,
                State = auxAddress.State,
                Zipcode = companyDTO.Zipcode
            };

            if (auxAddress.Street != "")
                address.Street = auxAddress.Street;
            else
            {
                if (companyDTO.Street != "" || companyDTO.Street.Equals("string") || companyDTO.Street != null)
                    address.Street = companyDTO.Street;
                else
                    return BadRequest("O campo Street é obrigatorio");
            }
            #endregion

            Company company = new Company()
            {
                Address = address,
                Cnpj = companyDTO.Cnpj,
                DtOpen = date,
                Name = companyDTO.Name,
                NameOPT = companyDTO.NameOPT,
                Status = companyDTO.Status
            };

            var insertCompany = _companyConnection.Insert(company);
            if (insertCompany != null)
                return Created("", "Inserido com sucesso!\n\n" + JsonConvert.SerializeObject(insertCompany, Formatting.Indented));
            return BadRequest("Erro ao inserir Companhia!");

        }

        [HttpPost("/SendToDeleted/{CNPJ}")]
        public ActionResult Delete(string CNPJ)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

            if (_companyConnection.FindByCnpj(CNPJ) != null || _companyConnection.FindByCnpjRestricted(CNPJ) != null)
            {
                if (_companyConnection.Delete(CNPJ))
                    return Ok("companhia deletada com sucesso!");
                else
                    return BadRequest("erro ao deletar");
            }
            return BadRequest("companhia inexistente");
        }

        [HttpPost("/SendToRestricted/{CNPJ}")]
        public ActionResult Restrict(string CNPJ)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

            if (_companyConnection.FindByCnpj(CNPJ) != null)
            {
                if (_companyConnection.Restrict(CNPJ))
                    return Ok("Companhia restrita com sucesso!");
                else
                    return BadRequest("erro ao restringir");
            }
            return BadRequest("Companhia inexistente");
        }

        [HttpPost("/UnrestrictCompany/{CNPJ}")]
        public ActionResult Unrestrict(string CNPJ)
        {
            if(CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

            if (_companyConnection.FindByCnpjRestricted(CNPJ) != null)
            {
                if (_companyConnection.Unrestrict(CNPJ) != null)
                    return Ok("Companhia retirada da lista de restritos com sucesso!");
                else
                    return BadRequest("erro ao retirar da lista de restritos");
            }
            return BadRequest("Companhia nao esta na lista de restritos");
        }

        [HttpPost("/UndeleteCompany/{CNPJ}")]
        public ActionResult UndeleteCompany(string CNPJ)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

            if (_companyConnection.FindByCnpjDeleted(CNPJ) != null)
            {
                if (_companyConnection.UndeleteCompany(CNPJ))
                    return Ok("Companhia retirada da lista de deletados com sucesso!");
                else
                    return BadRequest("erro ao retirar da lista de deletados");
            }
            return BadRequest("Companhia nao esta na lista de deletados");
        }

        [HttpPut("/UpdateNameOPT/{CNPJ},{NameOPT}")]
        public ActionResult UpdateName(string CNPJ, string NameOPT)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

            var company = _companyConnection.FindByCnpj(CNPJ);
            if (company != null)
            {
                company.NameOPT = NameOPT;
                if (_companyConnection.Update(CNPJ, company))
                    return Ok("NomeOPT do Companhia atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o nomeOPT da Companhia");
            }

            return BadRequest("Companhia nao esta na lista");
        }

        [HttpPut("/UpdateAddress/{CNPJ}")]
        public ActionResult UpdateAddress(string CNPJ, Address address)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");

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

            var company = _companyConnection.FindByCnpj(CNPJ);
            if (company != null)
            {
                company.Address = address;
                if (_companyConnection.Update(CNPJ, company)!=null)
                    return Ok("Endereço da Companhia atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o endereço da Companhia");
            }

            return BadRequest("Companhia nao esta na lista");
        }

        [HttpPut("/ChangeStatus/{CNPJ}")]
        public ActionResult ChangeStatus(string CNPJ)
        {
            if (CNPJ == null || CNPJ.Equals("string") || CNPJ == "")
                return BadRequest("CNPJ não informado!");

            CNPJ = CNPJ.Replace("%2F", "/");

            if (!CnpjValidation.Validate(CNPJ))
                return BadRequest("CNPJ invalido");


            var company = _companyConnection.FindByCnpj(CNPJ);
            if (company != null)
            {
                company.Status = !company.Status;
                if (_companyConnection.Update(CNPJ, company))
                    return Ok("Status da Companhia atualizado com sucesso!");
                else
                    return BadRequest("erro ao atualizar o status da Companhia");
            }

            return BadRequest("companhia nao esta na lista");
        }
    }
}
