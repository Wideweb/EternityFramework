using System;

namespace EternityFramework.Migrations.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationOrderAttribute : Attribute
    {
        public long MigrationOrder { get; set; }
    }
}
