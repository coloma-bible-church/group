namespace Group.WebApi.Services.Azure.Repositories
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