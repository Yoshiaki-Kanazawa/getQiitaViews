using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TimeZoneConverter;

namespace kanazawa.Function
{
    public static class get_qiita_views
    {
        [FunctionName("get_qiita_views")]
        public static async void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            TimeZoneInfo jstTimeZone = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");
            DateTime utcTime = DateTime.UtcNow;
            DateTime jstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, jstTimeZone);
            log.LogInformation($"C# Timer trigger function executed at: {jstTime}");

            string url = "https://qiita.com/api/v2/users/" + Parameter.getQiitaUserName() + "/items";
            string json = await GetJson(url);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<QiitaInformationModel> models = JsonConvert.DeserializeObject<List<QiitaInformationModel>>(json, settings);

            string getViewsCountUrl;
            foreach (var model in models)
            {
                getViewsCountUrl = "https://qiita.com/api/v2/items/" + model.Id;
                model.PageViewsCount = JsonConvert.DeserializeObject<QiitaInformationModel>(await GetJson(getViewsCountUrl)).PageViewsCount;
                log.LogInformation($"title: {model.Title}");
                log.LogInformation($"views: {model.PageViewsCount}");
            }

            var connectionString = Parameter.getConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    Database.checkMasterData(models, log, connection);
                    Database.saveData(models, jstTime, log, connection);
                }
                catch (Exception exception)
                {
                    log.LogInformation(exception.Message);
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private static async Task<string> GetJson(string url)
        {
            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Parameter.getQiitaAccessToken());

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }
    }
}