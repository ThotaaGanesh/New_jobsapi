namespace JobsApi.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> TriggerFunction(string functionURL)
        {
            ////https://fs-whatsupwebhook.azurewebsites.net/api/SendEmails?code=NRgabhXUyhVcqc3cm_tkwg2zQXsTxAWumKWE0bay15z0AzFuhN5Raw==
            ////https://fs-whatsupwebhook.azurewebsites.net/api/SendWhatsAppMessages?code=v8yZUNuMxSFvCgei7Kbza3Vf6AZKM7ocj8Jx1Y8gaiTvAzFudd2PZw==
            //var functionUrl = $"https://<your-function-app>.azurewebsites.net/api/{functionName}?code=<your-function-key>";
            HttpResponseMessage response = await _httpClient.GetAsync(functionURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}
