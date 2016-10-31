using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface ITradeManager
    {
        Data.Trade[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds);
        IQueryable<Data.Trade> GetChildren(string parentNo);
        Data.Trade GetEnabled(string no);
        IQueryable<Data.Trade> EnabledTrades { get; }
        void Delete(long uid, string[] nos);
        void Edit(long uid, string no, IClassEditInfo info);
    }

    internal class TradeManager : ITradeManager
    {
        private readonly Data.ITradeRepository _TradeRepository;

        public TradeManager(Data.ITradeRepository tradeRepository)
        {
            _TradeRepository = tradeRepository;
        }

        public Data.Trade[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds)
        {
            ExceptionHelper.ThrowIfNotId(@operator, "@operator");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(parentNo, "parentNo");
            if (adds == null) adds = new IClassEditInfo[0];
            adds = adds.Where(a => a != null).ToArray();
            ExceptionHelper.ThrowIfNullOrEmpty(adds, "adds");
            for (int i = 0; i < adds.Length; i++)
            {
                ExceptionHelper.ThrowIfNullOrWhiteSpace(adds[i].Name, String.Format("第{0}项的名称为空", i));
            }
            parentNo = parentNo.Trim();
            var parent = _TradeRepository.EnabledTrades.FirstOrDefault(c => c.no == parentNo);
            if (parent == null)
                throw new FlhException(ErrorCode.NotExists, "父级不存在或已被删除");

            var fullName = parent.full_name.Split(',');
            var fullNameEn = (parent.full_name_en??string.Empty).Split(',');

            using (var scope = new System.Transactions.TransactionScope())
            {
                var noLength = parent.no.Length + 4;
                var maxChild = _TradeRepository
                                .Entities
                                .Where(c => c.no.StartsWith(parent.no) && c.no.Length == noLength)
                                .OrderByDescending(c => c.no)
                                .FirstOrDefault();

                var num = 1;
                if (maxChild != null)
                {
                    num = Convert.ToInt32(maxChild.no.Substring(parent.no.Length)) + 1;
                }

                var addEntites = new List<Data.Trade>();
                foreach (var item in adds)
                {
                    var entity = new Data.Trade
                    {
                        enabled = true,
                        created = DateTime.Now,
                        creater = @operator,
                        name = item.Name.Trim(),
                        name_en =( item.EnName??string.Empty).Trim(),
                        no = parent.no + num.ToString().PadLeft(4, '0'),
                        order_by = item.Order,
                        updated = DateTime.Now,
                        updater = @operator,
                    };
                    entity.full_name_en = fullNameEn.Length == 0 ? entity.name_en : String.Join(",", fullNameEn.Concat(new[] { entity.name_en }));
                    entity.full_name = fullName.Length == 0 ? entity.name : String.Join(",", fullName.Concat(new[] { entity.name }));
                    num++;
                    addEntites.Add(entity);
                }
                _TradeRepository.AddRange(addEntites);
                _TradeRepository.SaveChanges();

                scope.Complete();

                return addEntites.ToArray();
            }
        }

        public IQueryable<Data.Trade> GetChildren(string parentNo)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(parentNo, "parentNo");
            parentNo = parentNo.Trim();
            var noLength = parentNo.Length + 4;
            return _TradeRepository
                            .EnabledTrades
                            .Where(c => c.no.StartsWith(parentNo) && c.no.Length == noLength);
        }


        public Data.Trade GetEnabled(string no)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(no, "no");
            no = no.Trim();
            var entity = _TradeRepository
                            .EnabledTrades
                            .FirstOrDefault(c => c.no == no);
            ExceptionHelper.ThrowIfNull(entity, "no", "no不存在");
            return entity;
        }
        public IQueryable<Data.Trade> EnabledTrades
        {
            get { return _TradeRepository.EnabledTrades; }
        }
        public void Delete(long uid, string[] nos)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            nos = (nos ?? Enumerable.Empty<string>()).Where(n => !String.IsNullOrWhiteSpace(n)).Distinct().ToArray();
            if (nos.Length>0)
            {
                _TradeRepository.Update(c => nos.Contains(c.no) && c.enabled, c => new Data.Trade { enabled = false, updated = DateTime.Now, updater = uid });
            }
        }

        public void Edit(long uid, string no, IClassEditInfo info)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            ExceptionHelper.ThrowIfNull(info,"info");
            var entity = GetEnabled(no);
            bool update = false;
            if (!String.IsNullOrWhiteSpace(info.Name)&&entity.name!=info.Name.Trim())
            {
                var oldName = entity.name;
                entity.name = info.Name.Trim();
                var entitys = _TradeRepository.Entities.Where(c => c.no.StartsWith(no)&&c.full_name!=null).ToArray();
                foreach(var item in entitys)
                {
                    item.full_name = Utility.ReplyFullName(item.full_name, oldName, info.Name);
                }
                update = true;
            }
            if (!String.IsNullOrWhiteSpace(info.EnName)&&entity.name_en!=info.EnName.Trim())
            {
                var oldEnName = entity.name_en;
                entity.name_en = info.EnName.Trim();
                var entitys = _TradeRepository.Entities.Where(c => c.no.StartsWith(no) && c.full_name_en != null).ToArray();
                foreach (var item in entitys)
                {
                    item.full_name_en = Utility.ReplyFullName(item.full_name_en, oldEnName, info.EnName);
                }
                update = true;
            }
            if (entity.order_by != info.Order)
            {
                entity.order_by = info.Order;
                update = true;
            }
            if (update)
                _TradeRepository.SaveChanges();
        }
    }
}