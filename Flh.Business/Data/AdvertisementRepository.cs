using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IAdvertisementRepository : Flh.Data.IRepository<Advertisement>
    {
        IQueryable<Advertisement> Advertisements { get; }
    }

    internal class AdvertisementRepository : DbSetRepository<FlhContext, Advertisement>, IAdvertisementRepository
    {
        public AdvertisementRepository(FlhContext context) : base(context) { }

        public IQueryable<Advertisement> Advertisements
        {
            get { return Entities.Where(u => u.isEnabled); }
        }
    }
}
