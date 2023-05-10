using System.Collections.Generic;
using DocumentValidator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.CompanyService.Services;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.PostOfficeService;

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
        public async Task<ActionResult<Company>> GetCompanyByCNPJ( string cnpj)
        {
            cnpj = cnpj.Replace("%2F", "/");
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


        [HttpPut("/UpdateNameOPT/{cnpj},{nameOPT}", Name = "Atualização do Nome Fantasia")]
        public async Task<ActionResult<string>> UpdateNameOPT(string cnpj, string nameOPT)
        {
            if (_companyConnection.FindAll().Where(x => x.Cnpj == cnpj) == null)
                return NotFound();

            _companyConnection.UpdateNameOPT(cnpj, nameOPT);

            return Accepted();
        }

        [HttpPatch("/status/{cnpj},{status}", Name = "Atualização do Status")]
        public async Task<ActionResult<string>> UpdateStatus(string cnpj, bool status)
        {
            if (_companyConnection.FindAll().Where(x => (x.Cnpj == cnpj) && (x.Status == false)).FirstOrDefault() is not null)
                return BadRequest("Não é possível fazer UPDATE neste documento pois o status dele está inativo!");

            _companyConnection.UpdateStatus(cnpj, status);

            return Accepted();
        }

        [HttpDelete("/companyactivated{cnpj}")]
        public async Task<ActionResult> DeleteCompany(string cnpj)
        {
            if (cnpj.Length != 14)
                return BadRequest("CNPJ informado não possui o formato necessário para realizar a deleção da companhia! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

            for (int i = 0; i < cnpj.Length; i++)
            {
                if ((cnpj[i] != '0') && (cnpj[i] != '1') && (cnpj[i] != '2') && (cnpj[i] != '3') && (cnpj[i] != '4') && (cnpj[i] != '5') && (cnpj[i] != '6') && (cnpj[i] != '7') && (cnpj[i] != '8') && (cnpj[i] != '9'))
                {
                    return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                }
            }

            if (_companyConnection.FindByCnpj(cnpj) == null)
                return BadRequest("O CNPJ informado não foi encontrado!");

            _companyConnection.Delete(cnpj);

            return Ok();
        }

        [HttpDelete("/companyrestricted/{cnpj}")]
        public async Task<ActionResult> DeleteCompanyRestricted(string cnpj)
        {
            if (cnpj.Length != 14)
                return BadRequest("CNPJ informado não possui o formato necessário para realizar a deleção da companhia! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

            for (int i = 0; i < cnpj.Length; i++)
            {
                if ((cnpj[i] != '0') && (cnpj[i] != '1') && (cnpj[i] != '2') && (cnpj[i] != '3') && (cnpj[i] != '4') && (cnpj[i] != '5') && (cnpj[i] != '6') && (cnpj[i] != '7') && (cnpj[i] != '8') && (cnpj[i] != '9'))
                {
                    return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                }
            }

            if (_companyConnection.FindByCnpjRestricted(cnpj) == null)
                return BadRequest("O CNPJ informado não foi encontrado!");

            //_companyConnection.DeleteByRestricted(cnpj);

            return Ok();
        }
    }
}
