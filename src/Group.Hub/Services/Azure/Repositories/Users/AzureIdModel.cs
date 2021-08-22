namespace Group.Hub.Services.Azure.Repositories.Users
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class AzureIdModel
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
    }
}