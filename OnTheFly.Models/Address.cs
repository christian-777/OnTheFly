using Newtonsoft.Json;
using OnTheFly.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace OnTheFly.Models
{
    public class Address
    {
        [JsonProperty("cep")]
        [StringLength(8)]
        public string Zipcode { get; set; }

        [JsonProperty("logradouro")]
        [StringLength(100)]
        public string? Street { get; set; }

        [StringLength(10)]
        public string? Complement { get; set; }

        [JsonProperty("localidade")]
        [StringLength(30)]
        public string City { get; set; }

        [JsonProperty("uf")]
        [StringLength(2)]
        public string State { get; set; }

        public int Number { get; set; }

        //public Address(AddressDTO addressDTO)
        //{
        //    Zipcode = addressDTO.Zipcode;
        //    Street = addressDTO.Street;
        //    City = addressDTO.City;
        //    State = addressDTO.State;
        //}
    }
}