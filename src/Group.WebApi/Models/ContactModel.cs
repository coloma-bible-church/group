namespace Group.WebApi.Models
{
    using System;

    [Serializable]
    public class ContactModel
    {
        public static class Kinds
        {
            public const string
                Phone = "phone";
        }

        public string Kind { get; set; } = Guid.NewGuid().ToString();

        public string Value { get; set; } = Guid.NewGuid().ToString();
    }
}