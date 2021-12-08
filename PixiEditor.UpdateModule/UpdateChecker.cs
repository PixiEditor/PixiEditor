using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule
{
    public class UpdateChecker
    {
        public UpdateChecker(string currentVersionTag, UpdateChannel channel)
        {
            CurrentVersionTag = currentVersionTag;
            Channel = channel;
        }

        public ReleaseInfo LatestReleaseInfo { get; private set; }

        public UpdateChannel Channel { get; set; }

        public string CurrentVersionTag { get; }

        /// <summary>
        ///     Compares version strings and returns true if newVer > originalVer.
        /// </summary>
        /// <param name="originalVer">Version to compare.</param>
        /// <param name="newVer">Version to compare with.</param>
        /// <returns>True if semantic version is higher.</returns>
        public static bool VersionBigger(string originalVer, string newVer)
        {
            if (!ParseVersionString(originalVer, out float ver1))
            {
                return false;
            }

            if (ParseVersionString(newVer, out float ver2))
            {
                return ver2 > ver1;
            }

            return false;
        }

        public async Task<bool> CheckUpdateAvailable()
        {
            LatestReleaseInfo = await GetLatestReleaseInfoAsync(Channel.ApiUrl);
            return CheckUpdateAvailable(LatestReleaseInfo);
        }

        public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
        {
            return latestRelease.WasDataFetchSuccessful && VersionBigger(CurrentVersionTag, latestRelease.TagName);
        }

        public bool IsUpdateCompatible(string[] incompatibleVersions)
        {
            return !incompatibleVersions.Select(x => x.Trim()).Contains(CurrentVersionTag.Trim());
        }

        public async Task<bool> IsUpdateCompatible()
        {
            string[] incompatibleVersions = await GetUpdateIncompatibleVersionsAsync(LatestReleaseInfo.TagName);
            return IsUpdateCompatible(incompatibleVersions);
        }

        public async Task<string[]> GetUpdateIncompatibleVersionsAsync(string tag)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
                HttpResponseMessage response = await client.GetAsync(string.Format(Channel.IncompatibleFileApiUrl, tag));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<string[]>(content);
                }
            }

            return Array.Empty<string>();
        }

        private static async Task<ReleaseInfo> GetLatestReleaseInfoAsync(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReleaseInfo>(content);
                }
            }

            return new ReleaseInfo(false);
        }

        private static bool ParseVersionString(string versionString, out float version)
        {
            return float.TryParse(versionString[..7].Replace(".", string.Empty).Insert(1, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out version);
        }
    }
}