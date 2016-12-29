using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IProductServiceFactory
    {
        IProductService CreateService(long pid);
    }
    class ProductServiceFactory : IProductServiceFactory
    {
         private readonly Data.IProductRepository _Repository;
         private readonly IProductSearchManager _SearchManager;
         public ProductServiceFactory(Data.IProductRepository repository,
             IProductSearchManager searchManager)
        {
            _Repository = repository;
             _SearchManager = searchManager;
        }
        public IProductService CreateService(long pid)
        {
            return new ProductService(pid,_Repository,_SearchManager);
        }
    }
    public interface IProductService
    {
        Data.IProduct Entity { get; }
        void AddViewCount();
    }

    class ProductService : IProductService
    {
         private readonly Data.IProductRepository _Repository;
         private readonly IProductSearchManager _SearchManager;
         private readonly Lazy<Data.Product> _LazyEntity;
         public ProductService(long pid,Data.IProductRepository repository,
             IProductSearchManager searchManager)
        {
            _Repository = repository;
             _SearchManager = searchManager;
            _LazyEntity = new Lazy<Data.Product>(() => {
                return _Repository.Entities.FirstOrDefault(d => d.pid == pid);
            });
        }

        public void AddViewCount()
        {
            _LazyEntity.Value.viewCount+=1;
            _Repository.SaveChanges();
            try
            {
                //搜索引擎
                var entityFromSearch = _SearchManager.SearchProductByPids(new[] { _LazyEntity.Value.pid }).FirstOrDefault();
                if (entityFromSearch != null)
                {
                    //每隔十次更新一下索引
                    if ((entityFromSearch.viewCount ?? 0) - (_LazyEntity.Value.viewCount ?? 0) >= 10)
                    {
                        _SearchManager.UpdateSearchIndex(_LazyEntity.Value);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.LoggerResolver.Current.Error(ex);
            }
        }
        
        public Data.IProduct Entity
        {
            get { return _LazyEntity.Value; }
        }
    }
}
