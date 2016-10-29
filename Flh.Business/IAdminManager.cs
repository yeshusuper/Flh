using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IAdminManager
    {
        void Add(long adminUid, long operatorUid);
        void Remove(long adminUid, long operatorUid);
        IQueryable<Data.Admin> EnabledEntities { get; }
        IQueryable<Data.Admin> AllEntities { get; }
    }
    class AdminManager : IAdminManager
    {
        private readonly Data.IAdminRepository _Repository;
        private readonly IAdminModifyHistoryManager _AdminModifyHistoryManager;
        public AdminManager(Data.IAdminRepository repository, IAdminModifyHistoryManager adminModifyHistoryManager)
        {
            _Repository = repository;
            _AdminModifyHistoryManager = adminModifyHistoryManager;
        }
        public void Add(long adminUid, long operatorUid)
        {
            //判断重复
            ExceptionHelper.ThrowIfNotId(adminUid, "adminUid");
            ExceptionHelper.ThrowIfNotId(operatorUid, "operatorUid");
            var admin = _Repository.Entities.FirstOrDefault(d => d.uid == adminUid);
            if(admin==null || !admin.enabled)
            {
                using (var scope = new System.Transactions.TransactionScope())
                {
                    if (admin == null)
                    {
                        var entity = Data.Admin.CreateNewInstance(adminUid);
                        entity.enabled = true;
                        entity.created = DateTime.Now;
                        _Repository.Add(entity);

                    }
                    else if (!admin.enabled)
                    {
                        admin.enabled = true;
                        admin.created = DateTime.Now;
                    }
                    _Repository.SaveChanges();
                    _AdminModifyHistoryManager.Add(new Data.AdminModifyHistory { opContent = "新增管理员", adminUid = adminUid, operatorUid = operatorUid });
                    scope.Complete();
                }
            }            
        }
        public void Remove(long adminUid, long operatorUid)
        {
            ExceptionHelper.ThrowIfNotId(adminUid, "adminUidID");
            ExceptionHelper.ThrowIfNotId(operatorUid, "operatorUid");
            //ExceptionHelper.ThrowIfTrue(adminUid == operatorUid, "adminUidID==operatorUid", "不能将自己的管理员权限移除");
            var entities = _Repository.EnabledAdmins.Where(d => d.uid == adminUid).ToArray();
            if (entities.Any())
            {
                using (var scope = new System.Transactions.TransactionScope())
                {
                    foreach (var item in entities)
                    {
                        item.enabled = false;
                        _AdminModifyHistoryManager.Add(new Data.AdminModifyHistory { opContent = "删除管理员", adminUid = item.uid, operatorUid = operatorUid });
                    }
                    _Repository.SaveChanges();
                    scope.Complete();
                }
            }

        }

        public IQueryable<Data.Admin> EnabledEntities
        {
            get
            {
                return _Repository.EnabledAdmins;
            }
        }


        public IQueryable<Data.Admin> AllEntities
        {
            get { return _Repository.Entities; }
        }

    }
}