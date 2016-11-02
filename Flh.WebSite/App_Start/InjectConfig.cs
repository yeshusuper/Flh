using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.WebSite
{
    public class InjectConfig
    {
        public static void Register()
        {
            DependencyResolver.SetResolver(new NinjectDependencyResolver(
                new Flh.Business.Inject.DataModule(),
                new Flh.Business.Inject.ServiceModule()));
        }
    }
}