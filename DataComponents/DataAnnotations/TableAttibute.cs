using System;

namespace EternityFramework.DataComponents.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
