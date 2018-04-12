using MonolithicExtensions.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace MonolithicExtensions.Windows
{
    /// <summary>
    /// Wraps a DIFactory object so that it can be used as an IDependencyResolver (for .NET web dependency injection stuff)
    /// </summary>
    public class DIResolver : DIFactory, IDependencyResolver
    {
        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
            //The DIFactory does not need to be disposed.
        }

        public object GetService(Type serviceType)
        {
            try { return Create(serviceType); }
            catch { return null; }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }
    }
}
