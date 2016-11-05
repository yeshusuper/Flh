using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IAdminRepository : Flh.Data.IRepository<Admin>
    {
        IQueryable<Admin> EnabledAdmins { get; }
    }

    internal class AdminRepository : DbSetRepository<FlhContext, Admin>, IAdminRepository
    {
        public AdminRepository(FlhContext context) : base(context) { }

        public IQueryable<Admin> EnabledAdmins
        {
            get { return Entities.Where(a => a.enabled); }
        }
    }
}
