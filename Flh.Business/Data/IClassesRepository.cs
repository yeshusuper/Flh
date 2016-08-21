using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IClassesRepository : Flh.Data.IRepository<Data.Classes>
    {
        IQueryable<Classes> EnabledClasses { get; }
    }

    internal class ClassesRepository : DbSetRepository<FlhContext, Classes>, IClassesRepository
    {
        public ClassesRepository(FlhContext context) : base(context) { }

        public IQueryable<Classes> EnabledClasses
        {
            get { return Entities.Where(c => c.enabled); }
        }
    }
}
