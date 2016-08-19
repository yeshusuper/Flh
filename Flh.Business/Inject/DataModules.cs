using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Flh.Data;

namespace Flh.Business.Inject
{
    public class DataModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<Data.User>>().To<Data.DbSetRepository<Data.FlhContext, Data.User>>();
            Bind<IRepository<Data.LoginHistory>>().To<Data.DbSetRepository<Data.FlhContext, Data.LoginHistory>>();
        }
    }
}
