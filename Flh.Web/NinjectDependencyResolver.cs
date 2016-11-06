using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ninject;

namespace Flh.Web
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private readonly Ninject.IKernel m_Kernel;

        public NinjectDependencyResolver(params Ninject.Modules.INinjectModule[] models)
        {
            m_Kernel = new Ninject.StandardKernel(new Ninject.NinjectSettings()
            {
                DefaultScopeCallback = ctx => System.Web.HttpContext.Current
            }, models);
        }

        public object GetService(Type serviceType)
        {
            return m_Kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return m_Kernel.GetAll(serviceType);
        }
    }
}
