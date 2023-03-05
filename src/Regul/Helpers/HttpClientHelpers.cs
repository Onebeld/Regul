namespace Regul.Helpers;

public static class HttpClientHelpers
{
    public static async Task<string> DownloadString(string url)
    {
        HttpClient client = new();

        using HttpResponseMessage response = await client.GetAsync(url);
        using HttpContent content = response.Content;
        
        return await content.ReadAsStringAsync();
    }
}