using System;

namespace EternityFramework.DataComponents.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttibute : Attribute
    {
        public string Name { get; set; }
    }
}
