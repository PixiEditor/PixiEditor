using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule
{
    public class UpdateChecker
    {
        public string ReleaseApiUrl = "https://api.github.com/repos/PixiEditor/PixiEditor/releases/latest";
        public string CurrentVersionTag { get; set; }

        public UpdateChecker(string currentVersionTag)
        {
            CurrentVersionTag = currentVersionTag;
        }

        public async Task<bool> CheckUpdateAvailable()
        {
            return CheckUpdateAvailable(await GetLatestReleaseInfo());
        }

        public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
        {
            return latestRelease.WasDataFetchSuccessfull && latestRelease.TagName != CurrentVersionTag;
        }

        public async Task<ReleaseInfo> GetLatestReleaseInfo()
        {
            using(HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
                var response = await client.GetAsync(ReleaseApiUrl);
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReleaseInfo>(content);
                }
            }
            return new ReleaseInfo(false);
        }
    }
}
