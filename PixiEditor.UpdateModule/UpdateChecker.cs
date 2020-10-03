using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule
{
    public class UpdateChecker
    {
        public const string ReleaseApiUrl = "https://api.github.com/repos/PixiEditor/PixiEditor/releases/latest";
        public string CurrentVersionTag { get; set; }

        public ReleaseInfo LatestReleaseInfo { get; set; }

        public UpdateChecker(string currentVersionTag)
        {
            CurrentVersionTag = currentVersionTag;
        }

        public async Task<bool> CheckUpdateAvailable()
        {
            LatestReleaseInfo = await GetLatestReleaseInfo();
            return CheckUpdateAvailable(LatestReleaseInfo);
        }

        public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
        {          
            return latestRelease.WasDataFetchSuccessfull && VersionBigger(CurrentVersionTag, latestRelease.TagName);
        }

        /// <summary>
        /// Compares version strings and returns true if newVer > originalVer
        /// </summary>
        /// <param name="originalVer"></param>
        /// <param name="newVer"></param>
        /// <returns></returns>
        public static bool VersionBigger(string originalVer, string newVer)
        {
            if(ParseVersionString(originalVer, out float ver1))
            {
                if (ParseVersionString(newVer, out float ver2))
                {
                    return ver2 > ver1;
                }
            }
            return false;
        }

        private static bool ParseVersionString(string versionString, out float version)
        {
            return float.TryParse(versionString.Replace(".", "").Insert(1, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out version);
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
