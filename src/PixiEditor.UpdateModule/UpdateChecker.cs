using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule;

public class UpdateChecker
{
    public UpdateChecker(string currentVersionTag, UpdateChannel channel)
    {
        CurrentVersionTag = currentVersionTag;
        Channel = channel;
    }

    public ReleaseInfo LatestReleaseInfo { get; private set; }

    private UpdateChannel _channel;

    public UpdateChannel Channel
    {
        get => _channel;
        set
        {
            bool changed = _channel != value;
            if (changed)
            {
                _channel = value;
                LatestReleaseInfo = null;
            }
        }
    }

    public string CurrentVersionTag { get; }

    /// <summary>
    ///     Compares version strings and returns true if newVer > originalVer.
    /// </summary>
    /// <param name="originalVer">Version to compare.</param>
    /// <param name="newVer">Version to compare with.</param>
    /// <returns>True if semantic version is higher.</returns>
    public static bool VersionDifferent(string originalVer, string newVer)
    {
        return ExtractVersionString(originalVer) != ExtractVersionString(newVer);
    }

    /// <summary>
    ///     Checks if originalVer is smaller than newVer
    /// </summary>
    /// <param name="originalVer">Version on the left side of the equation</param>
    /// <param name="newVer">Version to compare to</param>
    /// <returns>True if originalVer is smaller than newVer.</returns>
    public static bool VersionSmaller(string originalVer, string newVer)
    {
        string normalizedOriginal = ExtractVersionString(originalVer);
        string normalizedNew = ExtractVersionString(newVer);

        if (normalizedOriginal == normalizedNew) return false;

        if (!Version.TryParse(normalizedOriginal, out Version original))
        {
            throw new ArgumentException($"Invalid version string: {normalizedOriginal}");
        }

        if (!Version.TryParse(normalizedNew, out Version newVersion))
        {
            throw new ArgumentException($"Invalid version string: {normalizedNew}");
        }

        return original < newVersion;
    }

    public async Task<bool> CheckUpdateAvailable()
    {
        LatestReleaseInfo = await GetLatestReleaseInfoAsync(Channel.ApiUrl);
        return CheckUpdateAvailable(LatestReleaseInfo);
    }
    
    public void SetLatestReleaseInfo(ReleaseInfo releaseInfo)
    {
        LatestReleaseInfo = releaseInfo;
    }

    public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
    {
        if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName)) return false;
        if (CurrentVersionTag == null) return false;

        return latestRelease.WasDataFetchSuccessful && VersionDifferent(CurrentVersionTag, latestRelease.TagName);
    }

    public bool IsUpdateCompatible(string[] incompatibleVersions)
    {
        string extractedVersion = ExtractVersionString(CurrentVersionTag);
        bool containsVersion = incompatibleVersions.Select(x => x.Trim()).Contains(extractedVersion);
        if (containsVersion)
        {
            return false;
        }

        Version biggestIncompatibleVersion = incompatibleVersions
            .Select(x => Version.TryParse(ExtractVersionString(x), out Version version) ? version : null)
            .Where(x => x != null)
            .OrderByDescending(x => x)
            .FirstOrDefault();

        if (biggestIncompatibleVersion == null)
        {
            return true;
        }

        Version currentVersion = Version.TryParse(ExtractVersionString(CurrentVersionTag), out Version version) ? version : null;

        bool biggestVersionBiggerThanCurrent =
            biggestIncompatibleVersion >= currentVersion;

        return !biggestVersionBiggerThanCurrent;
    }

    public async Task<bool> IsUpdateCompatible()
    {
        string[] incompatibleVersions = await GetUpdateIncompatibleVersionsAsync(LatestReleaseInfo.TagName);
        bool isDowngrading = VersionSmaller(LatestReleaseInfo.TagName, CurrentVersionTag);
        return
            IsUpdateCompatible(incompatibleVersions) &&
            !isDowngrading; // Incompatible.json doesn't support backwards compatibility, thus downgrading always means update is not compatble
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

    private static string ExtractVersionString(string versionString)
    {
        if (string.IsNullOrEmpty(versionString)) return string.Empty;

        for (int i = 0; i < versionString.Length; i++)
        {
            if (!char.IsDigit(versionString[i]) && versionString[i] != '.')
            {
                return versionString[..i];
            }
        }

        return versionString;
    }
}
