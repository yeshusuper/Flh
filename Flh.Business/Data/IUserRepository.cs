using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IUserRepository : Flh.Data.IRepository<User>
    {
        IQueryable<User> EnabledUsers { get; }
    }

    internal class UserRepository : DbSetRepository<FlhContext, User>, IUserRepository
    {
        public UserRepository(FlhContext context) : base(context) { }

        public IQueryable<User> EnabledUsers
        {
            get { return Entities.Where(u => u.enabled); }
        }
    }
}
