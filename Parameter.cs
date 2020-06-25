using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace kanazawa.Function
{
    public class Parameter
    {
        private static string keyVaultEndpoint1 = "https://kanazawa-function-kv.vault.azure.net/secrets/Qiita-Access-Token/2bb6c74aa9ef4f8cb9e693618bc9b475";
        private static string keyVaultEndpoint2 = "https://kanazawa-function-kv.vault.azure.net/secrets/Qiita-User-Name/c155d47402bb40bfacc4da635f833033";
        private static string keyVaultEndpoint3 = "https://kanazawa-function-kv.vault.azure.net/secrets/Conection-String/9523cb1926ed449eb5c5c9606fa8d67a";

        public async static Task<string> getQiitaAccessToken(){
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var keyvaultSecret = await keyVaultClient.GetSecretAsync(keyVaultEndpoint1).ConfigureAwait(false);
            return keyvaultSecret.Value;
        }

        public async static Task<string> getQiitaUserName(){
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var keyvaultSecret = await keyVaultClient.GetSecretAsync(keyVaultEndpoint2).ConfigureAwait(false);
            return keyvaultSecret.Value;
        }

        public async static Task<string> getConnectionString(){
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var keyvaultSecret = await keyVaultClient.GetSecretAsync(keyVaultEndpoint3).ConfigureAwait(false);
            return keyvaultSecret.Value;
        }
    }
}