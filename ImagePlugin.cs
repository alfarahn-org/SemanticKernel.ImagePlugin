using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace SemanticKernel.Plugins;

public class ImagePlugin
{
    private const string ApiVersion = "2024-02-01"; 
    private const string DeploymentId = "Dalle3";

    [KernelFunction, Description("Generates an image from a prompt")]
    public async Task<string> GenerateImageAsync([Description("Prompt describing the image to generate.")] string prompt)
    {
        var url = $"{Configuration.ENDPOINT}/openai/deployments/{DeploymentId}/images/generations?api-version={ApiVersion}";

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("api-key", Configuration.KEY);

            dynamic body = new
            {
                prompt = prompt,
                size = "1024x1024",
                n = 1,
                quality = "hd",
                style = "vivid",
                response_format = "url"
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(result);

                if (data?.data != null && data?.data.Count > 0)
                {
                    string imageUrl = data.data[0].url;
                    OpenUrlInBrowser(imageUrl);
                    return $@"Image generated successfully and is already shown in browser with prompt: {prompt}";
                }
                else
                {
                    throw new HttpRequestException("Image generation failed, no data returned.");
                }
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
            }
        }
    }

    public void OpenUrlInBrowser(string url)
    {
        try
        {
            ProcessStartInfo psi = new ()
            {
                FileName = url,
                UseShellExecute = true 
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while trying to open the URL: " + ex.Message);
        }
    }

}
