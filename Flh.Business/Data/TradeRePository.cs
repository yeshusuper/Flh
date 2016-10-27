using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface ITradeRepository : Flh.Data.IRepository<Data.Trade>
    {
        IQueryable<Trade> EnabledTrades { get; }
    }

    internal class TradeRepository : DbSetRepository<FlhContext, Trade>, ITradeRepository
    {
        public TradeRepository(FlhContext context) : base(context) { }

        public IQueryable<Trade> EnabledTrades
        {
            get { return Entities.Where(c => c.enabled); }
        }
    }
}
