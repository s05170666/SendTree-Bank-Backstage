using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Web;

namespace 專題Employee_Version1.services
{
    public class CatService
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _catApiUrl = " https://api.thecatapi.com/v1/images/search?limit=10&breed_ids=beng&api_key=";

        public async Task<string> GetCatImageUrlAsync()
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync(_catApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JArray catData = JArray.Parse(json);
                    int randomIndex = new Random().Next(0, catData.Count);
                    string imageUrl = catData[randomIndex]["url"].ToString();

                    return imageUrl;
                }
                else
                {
                    return "No cat image available.";
                }
            }
            catch (Exception ex)
            {
                // Handle exception appropriately (log, rethrow, etc.)
                Console.WriteLine($"Error fetching cat image: {ex.Message}");
                return "Error fetching cat image.";
            }
        }
    }
}