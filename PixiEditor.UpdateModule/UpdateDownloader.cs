using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PixiEditor.UpdateModule
{
    public static class UpdateDownloader
    {
        public static readonly string DownloadLocation = AppDomain.CurrentDomain.BaseDirectory;

        public static async Task DownloadReleaseZip(ReleaseInfo release)
        {
            var matchingAsset = GetMatchingAsset(release);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PixiEditor");
                client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
                var response = await client.GetAsync(matchingAsset.Url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(Path.Join(DownloadLocation, $"update-{release.TagName}.zip"), bytes);
                }
            }
        }

        private static Asset GetMatchingAsset(ReleaseInfo release)
        {
            var arch = IntPtr.Size == 8 ? "x64" : "x86";
            return release.Assets.First(x => x.ContentType == "application/x-zip-compressed"
                                             && x.Name.Contains(arch));
        }
    }
}