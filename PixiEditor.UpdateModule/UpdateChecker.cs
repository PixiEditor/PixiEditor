using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule
{
    public class UpdateChecker
    {
        private const string ReleaseApiUrl = "https://api.github.com/repos/PixiEditor/PixiEditor/releases/latest";

        public UpdateChecker(string currentVersionTag)
        {
            CurrentVersionTag = currentVersionTag;
        }

        public ReleaseInfo LatestReleaseInfo { get; private set; }

        private string CurrentVersionTag { get; }

        /// <summary>
        ///     Compares version strings and returns true if newVer > originalVer.
        /// </summary>
        /// <param name="originalVer" />
        /// <param name="newVer"></param>
        /// <returns></returns>
        public static bool VersionBigger(string originalVer, string newVer)
        {
            if (!ParseVersionString(originalVer, out var ver1))
            {
                return false;
            }

            if (ParseVersionString(newVer, out var ver2))
            {
                return ver2 > ver1;
            }

            return false;
        }

        public async Task<bool> CheckUpdateAvailable()
        {
            LatestReleaseInfo = await GetLatestReleaseInfo_Async();
            return CheckUpdateAvailable(LatestReleaseInfo);
        }

        public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
        {
            return latestRelease.WasDataFetchSuccessful && VersionBigger(CurrentVersionTag, latestRelease.TagName);
        }

        private static async Task<ReleaseInfo> GetLatestReleaseInfo_Async()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
                var response = await client.GetAsync(ReleaseApiUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReleaseInfo>(content);
                }
            }

            return new ReleaseInfo(false);
        }

        private static bool ParseVersionString(string versionString, out float version)
        {
            return float.TryParse(versionString.Replace(".", string.Empty).Insert(1, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out version);
        }
    }
}