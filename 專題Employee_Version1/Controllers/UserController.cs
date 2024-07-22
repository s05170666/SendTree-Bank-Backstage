using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using 專題Employee_Version1.Models.ViewModels;
using 專題Employee_Version1.services;

public class UserController : Controller
{
    private readonly string apiKey = "CWA-52FEB868-8B04-4217-8172-E3172489AF0E";
    private readonly string apiUrl = "https://opendata.cwa.gov.tw/api/v1/rest/datastore/F-D0047-007?Authorization=";
    private readonly string jokeUrl = "https://v2.jokeapi.dev/joke/Dark";
    private readonly NewsService _newsService = new NewsService();
    private readonly CatService _catService = new CatService();

    public async Task<ActionResult> Index()
    {
        
        var articles = await _newsService.GetTaiwanBusinessNewsAsync();
        ViewBag.Articles = articles;

        //取得笑話
        var joke = await GetJokeAsync();
        ViewBag.Joke = joke;
        //取得貓咪圖片
        string imageUrl = await _catService.GetCatImageUrlAsync();
        ViewBag.CatImage = imageUrl;
        return View();
    }

   

    private async Task<string> GetJokeAsync()
    {
        var client = new HttpClient();
        var response = await client.GetStringAsync(jokeUrl);
        var json = JObject.Parse(response);
        if (json["type"].ToString() == "single")
        {
            return json["joke"].ToString();
        }
        else if (json["type"].ToString() == "twopart")
        {
            return $"{json["setup"]}\n{json["delivery"]}";
        }

        return "No joke available.";
    }


    [HttpPost]
    public async Task<ActionResult> Index(string selectedCity)
    {
        ViewBag.SelectCityList = await GetCitySelectListAsync();
        ViewBag.WeatherData = await GetWeatherDataAsync(selectedCity);
        return View();
    }

    private async Task<List<SelectListItem>> GetCitySelectListAsync()
    {
        var client = new HttpClient();
        var response = await client.GetStringAsync(apiUrl + apiKey);
        var json = JObject.Parse(response);

        var locations = json["records"]["locations"];
        var cityList = new List<SelectListItem>();

        foreach (var location in locations)
        {
            foreach (var locationData in location["location"])
            {
                cityList.Add(new SelectListItem
                {
                    Text = locationData["locationName"].ToString(),
                    Value = locationData["locationName"].ToString()
                });
            }

        }

        return cityList;
    }

    private async Task<string> GetWeatherDataAsync(string city = "龍潭區")
    {
        var client = new HttpClient();
        var response = await client.GetStringAsync(apiUrl + apiKey);
        var json = JObject.Parse(response);
        var locations = json["records"]["locations"];

        // 找到指定城市的天氣數據
        var weatherData = locations
            .SelectMany(l => l["location"])
            .Where(l => l["locationName"].ToString() == city)
            .SelectMany(l => l["weatherElement"])
            .FirstOrDefault();

        var result = new
        {
            City = city,
            WeatherData = weatherData
        };

        return result?.ToString();
    }
}

