using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.FollowUpRecord
{
    public interface IFollowUpRecordManager
    {
        IQueryable<Data.FollowUpRecord> FollowUpRecords { get; }
        IQueryable<Data.FollowUpRecord> GetFollowUpRecords(long uid);
        IFollowUpRecordService CreateService(long rid);
        IFollowUpRecordService Add(long uid, long administrator, string content, FollowUpRecordKinds kind);
        void Delete(long updater, long[] rids);

    }
    class FollowUpRecordManager : IFollowUpRecordManager
    {
        private readonly Data.IFollowUpRecordRepository _FollowUpRecordRepository;
        public FollowUpRecordManager(Data.IFollowUpRecordRepository followUpRecordRepository)
        {
            _FollowUpRecordRepository = followUpRecordRepository;
        }
        public IQueryable<Data.FollowUpRecord> FollowUpRecords
        {
            get { return _FollowUpRecordRepository.FollowUpRecords; }
        }

        public IQueryable<Data.FollowUpRecord> GetFollowUpRecords(long uid)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            return _FollowUpRecordRepository.FollowUpRecords.Where(r => r.uid == uid);
        }

        public IFollowUpRecordService CreateService(long rid)
        {
            return new FollowUpRecordService(rid, _FollowUpRecordRepository);
        }

        public IFollowUpRecordService Add(long uid, long administrator, string content, FollowUpRecordKinds kind)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            ExceptionHelper.ThrowIfNotId(administrator, "administrator");
            ExceptionHelper.ThrowIfNullOrEmpty(content, "content");
            var entity = new Data.FollowUpRecord
            {
                created = DateTime.Now,
                uid = uid,
                kind = kind,
                content = content,
                administrator = administrator,
                isEnabled = true,
                updated = DateTime.Now,
                updater = administrator,
            };
            _FollowUpRecordRepository.Add(entity);
            _FollowUpRecordRepository.SaveChanges();
            return CreateService(entity.rid);
        }
        public void Delete(long updater, long[] rids)
        {
            ExceptionHelper.ThrowIfNotId(updater, "updater");
            rids = (rids ?? Enumerable.Empty<long>()).Where(id => id > 0).Distinct().ToArray();
            if (rids.Length>0)
            {
                _FollowUpRecordRepository.Update(r => rids.Contains(r.rid) && r.isEnabled, r => new Data.FollowUpRecord { isEnabled = false, updated = DateTime.Now, updater = updater });
            }
        }
    }
}
