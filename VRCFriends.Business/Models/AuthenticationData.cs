using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VRCFriends.Business.Models
{
    [DataContract]
    public class AuthenticationData
    {
        [DataMember]
        [JsonPropertyName("auth_cookie")]
        public Cookie AuthCookie { get; set; }

        [DataMember]
        [JsonPropertyName("two_factor_cookie")]
        public Cookie TwoFactorCookie { get; set; }
    }
}
