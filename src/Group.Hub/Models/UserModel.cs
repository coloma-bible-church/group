namespace Group.Hub.Models
{
    using System;

    [Serializable]
    public class UserModel
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();

        public ContactModel[] Contacts { get; set; } = Array.Empty<ContactModel>();
    }
}