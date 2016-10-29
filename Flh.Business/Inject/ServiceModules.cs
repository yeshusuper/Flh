using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace Flh.Business.Inject
{
    public class ServiceModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IUserManager>().To<UserManager>();

            Bind<Admins.IAdminManager>().To<Admins.AdminManager>();
            Bind<IClassesManager>().To<ClassesManager>();
            Bind<IAdminModifyHistoryManager>().To<AdminModifyHistoryManager>();
            Bind<IAdminManager>().To<AdminManager>();
            Bind<ITradeManager>().To<TradeManager>();
            Bind<IAreaManager>().To<AreaManager>();
            Bind<IProductManager>().To<ProductManager>();
            
        }
    }
}
