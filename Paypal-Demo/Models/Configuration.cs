using PayPal.Api;
using System.Collections.Generic;

namespace Paypal_Demo.Models
{
    public static class Configuration
    {
        public static readonly string ClientId;
        public static readonly string ClientSecret;

        static Configuration()
        {
            var config = GetConfig();
            ClientId = config["clientId"];
            ClientSecret = config["clientSecret"];
        }

        public static Dictionary<string, string> GetConfig()
        {
            return ConfigManager.Instance.GetProperties();
        }

        private static string GetAccessToken()
        {
            // getting accesstocken from paypal                
            string accessToken = new OAuthTokenCredential
        (ClientId, ClientSecret, GetConfig()).GetAccessToken();

            return accessToken;
        }

        public static APIContext GetApiContext()
        {
            var apiContext = new APIContext(GetAccessToken()) {Config = GetConfig()};
            return apiContext;
        }
    }
}