using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IAreaRepository : Flh.Data.IRepository<Data.Area>
    {
        IQueryable<Area> EnabledAreas { get; }
    }

    internal class AreaRepository : DbSetRepository<FlhContext, Area>, IAreaRepository
    {
        public AreaRepository(FlhContext context) : base(context) { }

        public IQueryable<Area> EnabledAreas
        {
            get { return Entities.Where(c => c.enabled); }
        }
    }
}
