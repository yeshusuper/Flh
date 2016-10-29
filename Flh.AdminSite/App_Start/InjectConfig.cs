using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite
{
    public class InjectConfig
    {
        private class WebModule : Ninject.Modules.NinjectModule
        {
            public override void Load()
            {
                Bind<Flh.IO.FileManager>().ToSelf();
                Bind<Flh.IO.IFileStore>().To<Flh.IO.SystemFileStroe>();
            }
        }

        public static void Register()
        {
            DependencyResolver.SetResolver(new NinjectDependencyResolver(
                new Flh.Business.Inject.DataModule(),
                new Flh.Business.Inject.ServiceModule()));
        }
    }
}