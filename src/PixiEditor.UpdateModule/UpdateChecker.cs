﻿using System;
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

        bool parsed = TryParseToFloatVersion(normalizedOriginal, out float orgFloat);
        if (!parsed) throw new Exception($"Couldn't parse version {originalVer} to float.");

        parsed = TryParseToFloatVersion(normalizedNew, out float newFloat);
        if (!parsed) throw new Exception($"Couldn't parse version {newVer} to float.");

        return orgFloat < newFloat;
    }

    private static bool TryParseToFloatVersion(string normalizedString, out float ver)
    {
        return float.TryParse(normalizedString.Replace(".", string.Empty).Insert(1, "."), NumberStyles.Any, CultureInfo.InvariantCulture, out ver);
    }

    public async Task<bool> CheckUpdateAvailable()
    {
        LatestReleaseInfo = await GetLatestReleaseInfoAsync(Channel.ApiUrl);
        return CheckUpdateAvailable(LatestReleaseInfo);
    }

    public bool CheckUpdateAvailable(ReleaseInfo latestRelease)
    {
        if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName)) return false;
        if (CurrentVersionTag == null) return false;
        
        return latestRelease.WasDataFetchSuccessful && VersionDifferent(CurrentVersionTag, latestRelease.TagName);
    }

    public bool IsUpdateCompatible(string[] incompatibleVersions)
    {
        return !incompatibleVersions.Select(x => x.Trim()).Contains(ExtractVersionString(CurrentVersionTag));
    }

    public async Task<bool> IsUpdateCompatible()
    {
        string[] incompatibleVersions = await GetUpdateIncompatibleVersionsAsync(LatestReleaseInfo.TagName);
        bool isDowngrading = VersionSmaller(LatestReleaseInfo.TagName, CurrentVersionTag);
        return IsUpdateCompatible(incompatibleVersions) && !isDowngrading; // Incompatible.json doesn't support backwards compatibility, thus downgrading always means update is not compatble
    }

    public async Task<string[]> GetUpdateIncompatibleVersionsAsync(string tag)
    {
        using HttpClient client = new ();
        
        client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
        HttpResponseMessage response = await client.GetAsync(string.Format(Channel.IncompatibleFileApiUrl, tag));
        
        if (response.StatusCode != HttpStatusCode.OK) return Array.Empty<string>();
        
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<string[]>(content);
    }

    private static async Task<ReleaseInfo> GetLatestReleaseInfoAsync(string apiUrl)
    {
        using HttpClient client = new();

        client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
        HttpResponseMessage response = await client.GetAsync(apiUrl);

        if (response.StatusCode != HttpStatusCode.OK) return new ReleaseInfo(false);

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ReleaseInfo>(content);
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
