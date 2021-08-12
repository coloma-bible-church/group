namespace Group
{
    using System;
    using JetBrains.Annotations;

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAttribute : Attribute
    {
        public RegisterAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; }
    }
}