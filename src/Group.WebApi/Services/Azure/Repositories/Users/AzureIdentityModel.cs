namespace Group.WebApi.Services.Azure.Repositories.Users
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class AzureIdentityModel
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}