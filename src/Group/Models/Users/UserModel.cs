namespace Group.Models.Users
{
    using System;

    [Serializable]
    public sealed class UserModel
    {
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }
    }
}