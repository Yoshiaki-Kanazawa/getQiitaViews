using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kanazawa.Function
{
    public static class get_qiita_views
    {
        [FunctionName("get_qiita_views")]
        public static async void Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Qiita APIのURL
            string url = "https://qiita.com/api/v2/users/" + Constants.Qiita_User_Name + "/items";
            // 投稿記事情報取得
            string json = await GetJson(url);

            // デシリアライズ時の設定
            var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

            // デシリアライズ
            List<QiitaInformationModel> models = JsonConvert.DeserializeObject<List<QiitaInformationModel>>(json, settings);

            // 各投稿記事のView数を取得
            string getViewsCountUrl;
            foreach (var model in models)
            {
                getViewsCountUrl = "https://qiita.com/api/v2/items/" + model.Id;
                model.PageViewsCount = JsonConvert.DeserializeObject<QiitaInformationModel>(await GetJson(getViewsCountUrl)).PageViewsCount;
                log.LogInformation($"title: {model.Title}");
                log.LogInformation($"views: {model.PageViewsCount}");
            }

        }

        private static async Task<string> GetJson(string url)
        {
            var httpClient = new System.Net.Http.HttpClient();
            // OAuth 2.0 Authorization Headerの設定
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Constants.Qiita_Access_Token);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}