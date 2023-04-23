using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Washouse.Web.Models
{
    public class GoogleAccessTokenResponse
    {
        [JsonProperty("access_token")] public string access_token { get; set; }
        [JsonProperty("id_token")] public string id_token { get; set; }
        [JsonProperty("expires_in")] public long expires_in { get; set; }
        [JsonProperty("token_type")] public string token_type { get; set; }
        [JsonProperty("scope")] public string scope { get; set; }
        [JsonProperty("refresh_token")] public string refresh_token { get; set; }
        [JsonProperty("payload")] public string payload { get; set; }
    }
}