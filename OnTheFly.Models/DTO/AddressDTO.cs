
using Newtonsoft.Json;

namespace OnTheFly.Models.DTO
{
    public class AddressDTO
    {
        public int Id { get; set; }

        [JsonProperty("pais")]
        public string? Country { get; set; }
        [JsonProperty("cep")]
        public string Zipcode { get; set; }
        [JsonProperty("bairro")]
        public string Neighborhood { get; set; }
        [JsonProperty("localidade")]
        public string City { get; set; }
        [JsonProperty("uf")]
        public string State { get; set; }
        [JsonProperty("logradouro")]
        public string Street { get; set; }
    }
}