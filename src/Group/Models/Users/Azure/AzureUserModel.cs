namespace Group.Models.Users.Azure
{
    using System;

    [Serializable]
    public sealed class AzureUserModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public UserModel? Model { get; set; }
    }
}