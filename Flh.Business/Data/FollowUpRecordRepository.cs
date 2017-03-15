using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IFollowUpRecordRepository : Flh.Data.IRepository<FollowUpRecord>
    {
        IQueryable<FollowUpRecord> FollowUpRecords { get; }
    }

    internal class FollowUpRecordRepository : DbSetRepository<FlhContext, FollowUpRecord>, IFollowUpRecordRepository
    {
        public FollowUpRecordRepository(FlhContext context) : base(context) { }

        public IQueryable<FollowUpRecord> FollowUpRecords
        {
            get { return Entities.Where(u => u.isEnabled); }
        }
    }

}
