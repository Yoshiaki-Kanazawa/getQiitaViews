using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace kanazawa.Function
{
    public static class get_qiita_views
    {
        [FunctionName("get_qiita_views")]
        public static async void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
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

            // DB接続文字列の取得
            var connectionString = Constants.Conection_String;

            // データ保存
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    // データベースの接続開始
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            foreach (var model in models)
                            {
                                using (var command = new SqlCommand() { Connection = connection, Transaction = transaction })
                                {
                                    // SQLの準備
                                    command.CommandText = @"INSERT INTO page_views_count VALUES (@ID, @CREATED_AT, @TITLE, @PAGE_VIEWS_COUNT)";
                                    command.Parameters.Add(new SqlParameter("@ID", model.Id));
                                    command.Parameters.Add(new SqlParameter("@CREATED_AT", DateTime.Now.ToString("yyyy/MM/dd HH")));
                                    command.Parameters.Add(new SqlParameter("@TITLE", model.Title));
                                    command.Parameters.Add(new SqlParameter("@PAGE_VIEWS_COUNT", model.PageViewsCount));

                                    // SQLの実行
                                    command.ExecuteNonQuery();

                                    log.LogInformation($"succeeded to insert data: {model.Title}");
                                }
                            }

                            // コミット
                            transaction.Commit();
                            log.LogInformation("Committed");
                        }
                        catch
                        {
                            // ロールバック
                            transaction.Rollback();
                            log.LogInformation("Rollbacked");
                            throw;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw;
                }
                finally
                {
                    // データベースの接続終了
                    connection.Close();
                }
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