using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flh.Data;

namespace Flh.Business.Data
{

    public interface IAdminModifyHistoryRepository : Flh.Data.IRepository<AdminModifyHistory>
    {
        //IQueryable<AdminModifyHistory> AdminModifyHistories { get; }
    }

    internal class AdminModifyHistoryRepository : DbSetRepository<FlhContext, AdminModifyHistory>, IAdminModifyHistoryRepository
    {
        public AdminModifyHistoryRepository(FlhContext context) : base(context) { }
    }
}
