using System;

namespace kanazawa.Function
{
    public class Parameter
    {
        public static string getQiitaAccessToken(){
            return Environment.GetEnvironmentVariable("Qiita-Access-Token");
        }

        public static string getQiitaUserName(){
            return Environment.GetEnvironmentVariable("Qiita-User-Name");
        }

        public static string getConnectionString(){
            return Environment.GetEnvironmentVariable("ConnectionString");
        }
    }
}