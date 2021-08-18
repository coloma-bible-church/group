namespace Group.WebApi.Models
{
    using System;

    [Serializable]
    public class ResourceModel
    {
        public ResourceModel(string id, string path)
        {
            Id = id;
            Path = path;
        }

        public string Id { get; }
        public string Path { get; }
    }
}