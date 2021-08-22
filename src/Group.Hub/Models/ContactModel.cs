namespace Group.Hub.Models
{
    using System;

    [Serializable]
    public class ContactModel
    {
        public string Kind { get; set; } = Guid.NewGuid().ToString();

        public string Value { get; set; } = Guid.NewGuid().ToString();
    }
}