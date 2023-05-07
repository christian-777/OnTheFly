using OnTheFly.Models;
using Newtonsoft.Json;

namespace OnTheFly.AirCraftService.Services
{
    public class CompanyService
    {
        static readonly HttpClient client = new HttpClient();
        public static async Task<Company> GetCompany(string cnpj)
        {
            try
            {
                //falta colocar a porta do microserviço de company
                HttpResponseMessage response = await CompanyService.client.GetAsync("https://localhost:5001/" + cnpj);
                response.EnsureSuccessStatusCode();
                string ender = await response.Content.ReadAsStringAsync();
                var company = JsonConvert.DeserializeObject<Company>(ender);
                return company;
            }
            catch (HttpRequestException e)
            {
                return null;
            }
        }

        internal static async Task<Company> PutCompany(Company comp)
        {
            try
            {
                //falta colocar a porta do microserviço de company
                HttpResponseMessage response = await CompanyService.client.PutAsJsonAsync("", comp);
                response.EnsureSuccessStatusCode();
                string ender = await response.Content.ReadAsStringAsync();
                var company = JsonConvert.DeserializeObject<Company>(ender);
                return company;
            }
            catch (HttpRequestException e)
            {
                return null;
            }
        }
    }
}
