using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flh.Business.Data;

namespace Flh.Business
{
    public interface IAdminModifyHistoryManager
    {
        void Add(Data.AdminModifyHistory entity);
    }
    class AdminModifyHistoryManager : IAdminModifyHistoryManager
    {
        private readonly Data.IAdminModifyHistoryRepository _Respsitory;
        public AdminModifyHistoryManager(Data.IAdminModifyHistoryRepository respsitory)
        {
            _Respsitory = respsitory;
        }
        public void Add(AdminModifyHistory entity)
        {
            ExceptionHelper.ThrowIfNull(entity, "entity");
            ExceptionHelper.ThrowIfNotId(entity.adminUid, "entity.adminUid");
            ExceptionHelper.ThrowIfNotId(entity.operatorUid, "entity.operatorUid");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(entity.opContent, "entity.opContent");
            entity.created = DateTime.Now;
            _Respsitory.Add(entity);
            _Respsitory.SaveChanges();
        }
    }
}
