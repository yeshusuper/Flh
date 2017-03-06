using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flh;
using Flh.Log;

namespace Flh.Business.Advertisement
{
    public interface IAdvertisementService
    {
        long aid { get; }
        string title { get; }
        string content { get; }
        string url { get; }
        int clickCount { get; }
        DateTime created { get; }
        long creater { get; }
        long updater { get; }
        DateTime updated { get; }
        bool isEnabled { get; }
        string image { get; }
        string position { get; }
        int order { get; }
        void Update(long uid, string title, string content, string url,  string image, string position,int order);
        void Delete(long uid);
        void Click();
    }
    class AdvertisementService : IAdvertisementService
    {
        private readonly Data.IAdvertisementRepository _AdvertisementRepository;
        private readonly long _Aid;
        private readonly Lazy<Data.Advertisement> _LazyAdvertisement;
        public AdvertisementService(long aid, Data.IAdvertisementRepository advertisementRepository)
        {
            ExceptionHelper.ThrowIfNotId(aid, "aid");
            _AdvertisementRepository = advertisementRepository;
            _Aid = aid;
            _LazyAdvertisement = new Lazy<Data.Advertisement>(() =>
            {
                var entity = _AdvertisementRepository.Entities.FirstOrDefault(a => a.aid == aid);
                if (entity == null)
                    throw new FlhException(ErrorCode.NotExists, "广告不存在");
                return entity;
            });
        }

        public long aid
        {
            get { return _Aid; }
        }

        public string title
        {
            get { return _LazyAdvertisement.Value.title; }
        }

        public string content
        {
            get { return _LazyAdvertisement.Value.content; ; }
        }

        public string url
        {
            get { return _LazyAdvertisement.Value.url; }
        }

        public int clickCount
        {
            get { return _LazyAdvertisement.Value.clickCount; }
        }

        public DateTime created
        {
            get {return  _LazyAdvertisement.Value.created; }
        }

        public long creater
        {
            get { return _LazyAdvertisement.Value.creater; }
        }

        public long updater
        {
            get { return _LazyAdvertisement.Value.updater; }
        }

        public DateTime updated
        {
            get { return _LazyAdvertisement.Value.updated; }
        }

        public bool isEnabled
        {
            get { return _LazyAdvertisement.Value.isEnabled; }
        }

        public string image
        {
            get { return _LazyAdvertisement.Value.image; }
        }

        public string position
        {
            get { return _LazyAdvertisement.Value.position; }
        }

        public void Update(long uid, string title, string content, string url, string image, string position,int order)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            _LazyAdvertisement.Value.position = (position ?? String.Empty).Trim();
            _LazyAdvertisement.Value.image = (image?? String.Empty).Trim();
            _LazyAdvertisement.Value.title = (title?? String.Empty).Trim();
            _LazyAdvertisement.Value.url = (url?? String.Empty).Trim();
            _LazyAdvertisement.Value.content = content;
            _LazyAdvertisement.Value.updated = DateTime.Now;
            _LazyAdvertisement.Value.updater = uid;
            _LazyAdvertisement.Value.orderBy=order;
            _AdvertisementRepository.SaveChanges();
        }

        public void Delete(long uid)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            _LazyAdvertisement.Value.isEnabled = false;
            _LazyAdvertisement.Value.updater = uid;
            _LazyAdvertisement.Value.updated = DateTime.Now;
            _AdvertisementRepository.SaveChanges();
        }
        public int order
        {
            get { return _LazyAdvertisement.Value.orderBy; }
        }
        public void Click()
        {
            _LazyAdvertisement.Value.clickCount += 1;
            _AdvertisementRepository.SaveChanges();
        }
    }
}
