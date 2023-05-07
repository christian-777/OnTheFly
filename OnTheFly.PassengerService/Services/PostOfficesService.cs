using Newtonsoft.Json;
using OnTheFly.Models.DTO;

namespace AndreTurismoApp.Services
{
    public class PostOfficesService
    {
        static readonly HttpClient address = new HttpClient();
        public async Task<AddressDTO> GetAddress(string cep)
        {
            try
            {
                HttpResponseMessage response = await PostOfficesService.address.GetAsync("https://viacep.com.br/ws/" + cep + "/json/");
                response.EnsureSuccessStatusCode();
                string ad = await response.Content.ReadAsStringAsync();
                var addressfull = JsonConvert.DeserializeObject<AddressDTO>(ad);
                return addressfull;

            }catch (HttpRequestException ex)
            {
                throw;
            }
        }
    }
}
