using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PixiEditor.UpdateModule
{
    public class ReleaseInfo
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }
        [JsonPropertyName("draft")]
        public bool IsDraft { get; set; }
        [JsonPropertyName("prerelease")]
        public bool IsPrerelease { get; set; }
        [JsonPropertyName("assets")]
        public Asset[] Assets { get; set; }
        public bool WasDataFetchSuccessfull { get; set; } = true;

        public ReleaseInfo() { }
        public ReleaseInfo(bool dataFetchSuccessfull)
        {
            WasDataFetchSuccessfull = dataFetchSuccessfull;
        }
    }
}
