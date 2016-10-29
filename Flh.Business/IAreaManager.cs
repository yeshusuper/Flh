using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IAreaManager
    {
        Data.Area[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds);
        IQueryable<Data.Area> GetChildren(string parentNo);
        Data.Area GetEnabled(string no);
        IQueryable<Data.Area> EnabledAreas { get; }
    }

    internal class AreaManager : IAreaManager
    {
        private readonly Data.IAreaRepository _AreaRepositor;

        public AreaManager(Data.IAreaRepository areaRepository)
        {
            _AreaRepositor = areaRepository;
        }

        public Data.Area[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds)
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
            var parent = _AreaRepositor.EnabledAreas.FirstOrDefault(c => c.area_no == parentNo);
            if (parent == null)
                throw new FlhException(ErrorCode.NotExists, "父级不存在或已被删除");

            var fullName = parent.area_full_name.Split(',');
            var fullNameEn = (parent.area_full_name_en ?? string.Empty).Split(',');

            using (var scope = new System.Transactions.TransactionScope())
            {
                var noLength = parent.area_no.Length + 4;
                var maxChild = _AreaRepositor
                                .Entities
                                .Where(c => c.area_no.StartsWith(parent.area_no) && c.area_no.Length == noLength)
                                .OrderByDescending(c => c.area_no)
                                .FirstOrDefault();

                var num = 1;
                if (maxChild != null)
                {
                    num = Convert.ToInt32(maxChild.area_no.Substring(parent.area_no.Length)) + 1;
                }

                var addEntites = new List<Data.Area>();
                foreach (var item in adds)
                {
                    var entity = new Data.Area
                    {
                        enabled = true,
                        created = DateTime.Now,
                        creater = @operator,
                        area_name = item.Name.Trim(),
                        area_name_en = (item.EnName ?? string.Empty).Trim(),
                        area_no = parent.area_no + num.ToString().PadLeft(4, '0'),
                        order_by = item.Order,
                        updated = DateTime.Now,
                        updater = @operator,
                    };
                    entity.area_full_name_en = fullNameEn.Length == 0 ? entity.area_name_en : String.Join(",", fullNameEn.Concat(new[] { entity.area_name_en }));
                    entity.area_full_name = fullName.Length == 0 ? entity.area_name : String.Join(",", fullName.Concat(new[] { entity.area_name }));
                    num++;
                    addEntites.Add(entity);
                }
                _AreaRepositor.AddRange(addEntites);
                _AreaRepositor.SaveChanges();

                scope.Complete();

                return addEntites.ToArray();
            }
        }

        public IQueryable<Data.Area> GetChildren(string parentNo)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(parentNo, "parentNo");
            parentNo = parentNo.Trim();
            var noLength = parentNo.Length + 4;
            return _AreaRepositor
                            .EnabledAreas
                            .Where(c => c.area_no.StartsWith(parentNo) && c.area_no.Length == noLength);
        }


        public Data.Area GetEnabled(string no)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(no, "no");
            no = no.Trim();
            var entity = _AreaRepositor
                            .EnabledAreas
                            .FirstOrDefault(c => c.area_no == no);
            ExceptionHelper.ThrowIfNull(entity, "no", "no不存在");
            return entity;
        }
        public IQueryable<Data.Area> EnabledAreas
        {
            get
            {
                return _AreaRepositor.EnabledAreas;
            }
        }
    }
}
