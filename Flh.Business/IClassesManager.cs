using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flh;

namespace Flh.Business
{
    public interface IClassEditInfo
    {
        string Name { get; }
        string EnName { get; }
        int Order { get; }
    }

    public interface IClassesManager
    {
        Data.Classes[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds);
        IQueryable<Data.Classes> GetChildren(string parentNo);
        Data.Classes GetEnabled(string no);
        IQueryable<Data.Classes> EnabledClasses { get; }
    }

    internal class ClassesManager : IClassesManager
    {
        private readonly Data.IClassesRepository _ClassesRepository;

        public ClassesManager(Data.IClassesRepository classesRepository)
        {
            _ClassesRepository = classesRepository;
        }

        public Data.Classes[] AddRange(long @operator, string parentNo, IClassEditInfo[] adds)
        {
            ExceptionHelper.ThrowIfNotId(@operator, "@operator");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(parentNo, "parentNo");
            if (adds == null) adds = new IClassEditInfo[0];
            adds = adds.Where(a => a != null).ToArray();
            ExceptionHelper.ThrowIfNullOrEmpty(adds, "adds");
            for (int i = 0; i < adds.Length; i++)
            {
                ExceptionHelper.ThrowIfNullOrWhiteSpace(adds[i].Name, String.Format("第{0}项的名称为空", i));
                ExceptionHelper.ThrowIfNullOrWhiteSpace(adds[i].EnName, String.Format("第{0}项的英文名称为空", i));
            }
            parentNo = parentNo.Trim();
            var parent = _ClassesRepository.EnabledClasses.FirstOrDefault(c => c.no == parentNo);
            if (parent == null)
                throw new FlhException(ErrorCode.NotExists, "父级不存在或已被删除");

            var fullName = parent.full_name.Split(',');
            var fullNameEn = parent.full_name_en.Split(',');

            using(var scope = new System.Transactions.TransactionScope())
            {
                var noLength = parent.no.Length + 4;
                var maxChild = _ClassesRepository
                                .Entities
                                .Where(c => c.no.StartsWith(parent.no) && c.no.Length == noLength)
                                .OrderByDescending(c => c.no)
                                .FirstOrDefault();

                var num = 1;
                if (maxChild != null)
                {
                    num = Convert.ToInt32(maxChild.no.Substring(parent.no.Length)) + 1;
                }

                var addEntites = new List<Data.Classes>();
                foreach (var item in adds)
                {
                    var entity = new Data.Classes
                    {
                        enabled = true,
                        created = DateTime.Now,
                        creater = @operator,
                        name = item.Name.Trim(),
                        name_en = item.EnName.Trim(),
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
                _ClassesRepository.AddRange(addEntites);
                _ClassesRepository.SaveChanges();

                scope.Complete();

                return addEntites.ToArray();
            }
        }

        public IQueryable<Data.Classes> GetChildren(string parentNo)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(parentNo, "parentNo");
            parentNo = parentNo.Trim();
            var noLength = parentNo.Length + 4;
            return _ClassesRepository
                            .EnabledClasses
                            .Where(c => c.no.StartsWith(parentNo) && c.no.Length == noLength);
        }


        public Data.Classes GetEnabled(string no)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(no, "no");
            no = no.Trim();
            var entity =  _ClassesRepository
                            .EnabledClasses
                            .FirstOrDefault(c => c.no == no);
            ExceptionHelper.ThrowIfNull(entity, "no", "no不存在");
            return entity;
        }
        public IQueryable<Data.Classes> EnabledClasses
        {
            get { return _ClassesRepository.EnabledClasses; }
        }
    }
}
