using Newtonsoft.Json;
using OnTheFly.Models;

namespace OnTheFly.SaleService.Services
{
    public class PassengerService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<Passenger> GetPassenger(string CPF)
        {
            HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5004/api/Passenger/" + CPF);
            if (!res.IsSuccessStatusCode) return null;

            string content = await res.Content.ReadAsStringAsync();
            Passenger? result = JsonConvert.DeserializeObject<Passenger>(content);

            return result;
        }

    }
}
