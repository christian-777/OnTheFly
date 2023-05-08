using Newtonsoft.Json;
using OnTheFly.Models;

namespace OnTheFly.Services
{
    public class PostOfficeService
    {
        static readonly HttpClient address = new HttpClient();

        public async Task<Address> GetAddress(string zipCode)
        {
            try
            {
                HttpResponseMessage response = await address.GetAsync("https://viacep.com.br/ws/" + zipCode + "/json/");
                response.EnsureSuccessStatusCode();
                string addressResponse = await response.Content.ReadAsStringAsync();
                var ad = JsonConvert.DeserializeObject<Address>(addressResponse);
                return ad;
            }
            catch (HttpRequestException e)
            {
                throw;
            }
        }
    }
}