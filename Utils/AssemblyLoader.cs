using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;

namespace DbMigratorCore.Utils
{
    public class AssemblyLoader : AssemblyLoadContext
    {
        // Not exactly sure about this
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var deps = DependencyContext.Default;
            var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
            var assembly = Assembly.Load(new AssemblyName(res.First().Name));
            return assembly;
        }
    }
}
