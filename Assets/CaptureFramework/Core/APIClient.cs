using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

class APIClient
{
    public static HttpClient GetHttpClient()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri($"http://{SettingsManager.Instance.serverIP}:{SettingsManager.Instance.serverPort + 1}/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
    public static async Task<Collection[]> GetCollectionsAsync()
    {
        using (HttpClient client = GetHttpClient())
        {
            Collection[] product = null;
            HttpResponseMessage response = await client.GetAsync("api/collections");

            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<Collection[]>();
            }
            return product;
        }
    }
}