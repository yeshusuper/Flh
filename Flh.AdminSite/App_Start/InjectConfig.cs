using Flh.Web;
using Flh.Web.Aliyun;
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
                if (AliyunHelper.AliyunAccessKey == null)
                    Bind<Flh.IO.IFileStore>().To<Flh.IO.SystemFileStroe>();
                else
                    Bind<Flh.IO.IFileStore>().ToConstant(new OSSService("flh-images"));
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