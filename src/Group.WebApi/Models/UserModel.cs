namespace Group.WebApi.Models
{
    using System;

    [Serializable]
    public class UserModel
    {
        public string? Name { get; set; }
        public ContactModel[] Contacts { get; set; } = Array.Empty<ContactModel>();
    }
}