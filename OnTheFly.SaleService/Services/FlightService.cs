using Newtonsoft.Json;
using OnTheFly.Models;

namespace OnTheFly.SaleService.Services
{
    public class FlightService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<Flight> GetFlight(string IATA, string RAB, DateTime departure)
        {
            HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5003/api/Flight/" + IATA + ", " + RAB + ", " + departure);
            if (!res.IsSuccessStatusCode) return new Flight();

            string content = await res.Content.ReadAsStringAsync();
            Flight? result = JsonConvert.DeserializeObject<Flight>(content);

            return result;
        }

    }
}
