using EternityFramework.DataAccess;
using EternityFramework.LinqToSql;
using EternityFramework.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace EternityFramework
{
    public class DbContext : IDisposable
    {
        private bool disposed = false;

        public DbQuery DbQuery { get; }

        public DbContext(string connectionString)
        {
            DbQuery = new DbQuery(connectionString);

            var properties = GetType()
                .GetProperties(/*BindingFlags.Public | BindingFlags.Instance*/)
                .Where(it => TypeSystem.IsTypeAssignable(it.PropertyType, typeof(DbQueryable<>)));

            foreach(var property in properties)
            {
                Type elementType = TypeSystem.GetElementType(property.PropertyType);
                var collectionInstance = Activator
                    .CreateInstance(typeof(DbQueryable<>).MakeGenericType(elementType), new object[] { DbQuery });
                property.SetValue(this, collectionInstance);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                DbQuery.Dispose();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~DbContext()
        {
            Dispose(false);
        }
    }
}
