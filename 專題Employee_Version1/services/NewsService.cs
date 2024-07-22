using NewsAPI.Constants;
using NewsAPI.Models;
using NewsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace 專題Employee_Version1.services
{
    public class NewsService
    {
        private static readonly string apiKey = "0d2f9e1670ea4036bb90df74d876507f"; // 你的 API 密钥
        private static readonly string country = "tw";
        private static readonly string category = "business";

        public async Task<List<Article>> GetTaiwanBusinessNewsAsync()
        {
            try
            {
                var newsApiClient = new NewsApiClient(apiKey);
                var articlesResponse = await newsApiClient.GetTopHeadlinesAsync(new TopHeadlinesRequest
                {
                    Country = Countries.TW,
                    Category = Categories.Business,

                });

                if (articlesResponse.Status == Statuses.Ok)
                {
                    return articlesResponse.Articles.ToList();
                }
                else
                {
                    // 處理 API 響應狀態不正確的情況
                    // 例如，可以記錄錯誤或返回一個空列表
                    return new List<Article>();
                }
            }
            catch (Exception ex)
            {
                // 處理異常情況，例如 API 請求失敗
                // 例如，可以記錄錯誤或返回一個空列表
                return new List<Article>();
            }
        }
    }
}