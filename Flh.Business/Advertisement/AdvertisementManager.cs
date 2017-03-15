using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Advertisement
{
    public interface IAdvertisementManager
    {
        IQueryable<Data.Advertisement> Advertisements { get; }
        IQueryable<Data.Advertisement> GetAdvertisement(string position);
        IAdvertisementService Add(string title, string content, string url, long creater, string image, string position, int order);
        IAdvertisementService CreateService(long aid);
        void Delete(long[] aids, long admin);
        string GetPositionName(string position);
    }
    class AdvertisementManager : IAdvertisementManager
    {
        private readonly Data.IAdvertisementRepository _AdvertisementRepository;
        public AdvertisementManager(Data.IAdvertisementRepository advertisementRepository)
        {
            _AdvertisementRepository = advertisementRepository;
        }
        public IQueryable<Data.Advertisement> Advertisements
        {
            get { return _AdvertisementRepository.Advertisements; }
        }

        public IQueryable<Data.Advertisement> GetAdvertisement(string position)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(position, "position", "请输入广告位置");
            return _AdvertisementRepository.Advertisements.Where(a => a.position == position);
        }

        public IAdvertisementService Add(string title, string content, string url, long creater, string image, string position, int order)
        {
            ExceptionHelper.ThrowIfNotId(creater, "creater");
            ExceptionHelper.ThrowIfNullOrEmpty(position, "position", "请输入广告位置");

            var entity = new Data.Advertisement
            {
                clickCount = 0,
                position = position.Trim(),
                isEnabled = true,
                content = (content ?? String.Empty).Trim(),
                created = DateTime.Now,
                creater = creater,
                image = (image ?? String.Empty).Trim(),
                title = (title ?? String.Empty).Trim(),
                updated = DateTime.Now,
                updater = creater,
                orderBy = order,
                url = (url ?? string.Empty).Trim(),
            };
            _AdvertisementRepository.Add(entity);
            _AdvertisementRepository.SaveChanges();
            return CreateService(entity.aid);
        }
        public IAdvertisementService CreateService(long aid)
        {
            ExceptionHelper.ThrowIfNotId(aid, "aid");
            return new AdvertisementService(aid, _AdvertisementRepository);
        }
        public void Delete(long[] aids, long admin)
        {
            ExceptionHelper.ThrowIfNotId(admin, "admin");
            aids = (aids ?? Enumerable.Empty<long>()).Where(id => id > 0).Distinct().ToArray();
            if (aids.Length > 0)
            {
                _AdvertisementRepository.Update(a => aids.Contains(a.aid), a => new Data.Advertisement { isEnabled = false, updated = DateTime.Now, updater = admin });
            }
        }
        public string GetPositionName(string position)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(position, "position");
            string Name = null;
            switch (position)
            {
                case "Index":
                    Name = "首页广告";
                    break;
            }
            return Name;
        }
    }
}