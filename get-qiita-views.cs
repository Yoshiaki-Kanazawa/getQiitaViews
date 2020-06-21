using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace kanazawa.Function
{
    public static class get_qiita_views
    {
        [FunctionName("get_qiita_views")]
        public static void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Qiita APIのURL
            string url = "https://qiita.com/api/v2/users/" + Constants.Qiita_User_Name + "/items";
            // 投稿記事情報取得
            string json = await GetJson(url);
            log.LogInformation($"json: {json}");

            // デシリアライズ
            List<QiitaInformationModel> models = JsonConvert.DeserializeObject<List<QiitaInformationModel>>(json);

            // 各投稿記事のView数を取得
            string getViewsCountUrl;
            foreach (var model in models)
            {
                getViewsCountUrl = "https://qiita.com/api/v2/items/" + model.Id;
                model.PageViewsCount = JsonConvert.DeserializeObject<QiitaInformationModel>(await GetJson(getViewsCountUrl)).PageViewsCount;
                log.LogInformation($"title: {model.Title}");
                log.LogInformation($"title: {model.PageViewsCount}");
            }

        }

        private static async Task<string> GetJson(string url)
        {
            HttpClient httpClient = new System.Net.Http.HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add("Authorization", $"BEARER {Constants.Qiita_Access_Token}");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}
