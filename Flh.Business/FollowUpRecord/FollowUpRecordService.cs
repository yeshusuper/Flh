using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.FollowUpRecord
{
    public interface IFollowUpRecordService
    {
         long Rid { get; }
         string Content { get;  }
         DateTime Created { get; }
         long Administrator { get; }
         long Uid { get; }
         bool IsEnabled { get;  }
         Flh.Business.FollowUpRecord.FollowUpRecordKinds Kind { get;  }
         void Delete(long uid);
    }
    class FollowUpRecordService : IFollowUpRecordService
    {
        private readonly Data.IFollowUpRecordRepository _FollowUpRecordRepository;
        private readonly Lazy<Data.FollowUpRecord> _LazyFollowUpRecord;
        private readonly long _Rid;
        public FollowUpRecordService(long rid,Data.IFollowUpRecordRepository followUpRecordRepository)
        {
            ExceptionHelper.ThrowIfNotId(rid, "rid");
            _Rid = rid;
            _FollowUpRecordRepository = followUpRecordRepository;
            _LazyFollowUpRecord = new Lazy<Data.FollowUpRecord>(() =>
            {
                var entity = _FollowUpRecordRepository.Entities.FirstOrDefault(r => r.rid == rid);
                if (entity == null)
                    throw new FlhException(ErrorCode.NotExists, "跟进记录不存在");
                return entity;
            });
        }
        public long Rid
        {
            get { return _Rid ; }
        }

        public string Content
        {
            get { return _LazyFollowUpRecord.Value.content; }
        }

        public DateTime Created
        {
            get { return _LazyFollowUpRecord.Value.created; }
        }

        public long Administrator
        {
            get { return _LazyFollowUpRecord.Value.administrator ; }
        }

        public long Uid
        {
            get { return _LazyFollowUpRecord.Value.uid; }
        }

        public bool IsEnabled
        {
            get { return _LazyFollowUpRecord.Value.isEnabled; }
        }

        public FollowUpRecordKinds Kind
        {
            get { return _LazyFollowUpRecord.Value.kind; ; }
        }


        public void Delete(long uid)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            if (IsEnabled)
            {
                _LazyFollowUpRecord.Value.isEnabled = false;
                _LazyFollowUpRecord.Value.updated = DateTime.Now;
                _LazyFollowUpRecord.Value.updater = uid;
                _FollowUpRecordRepository.SaveChanges();
            }
        }
    }
}
