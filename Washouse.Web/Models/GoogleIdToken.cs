using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Washouse.Web.Models
{
    public class GoogleIdToken
    {
        [JsonProperty("iss")] public string iss { get; set; }
        [JsonProperty("azp")] public string azp { get; set; }
        [JsonProperty("sub")] public string sub { get; set; }
        [JsonProperty("aud")] public string aud { get; set; }
        [JsonProperty("hd")] public string hd { get; set; }
        [JsonProperty("email")] public string email { get; set; }
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("email_verified")] public bool email_verified { get; set; }
        [JsonProperty("at_hash")] public string at_hash { get; set; }
        [JsonProperty("picture")] public string picture { get; set; }
        [JsonProperty("given_name")] public string given_name { get; set; }
        [JsonProperty("family_name")] public string family_name { get; set; }
        [JsonProperty("locale")] public string locale { get; set; }
        [JsonProperty("nonce")] public string nonce { get; set; }
        [JsonProperty("iat")] public long iat { get; set; }
        [JsonProperty("exp")] public long exp { get; set; }
    }
}