using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IProductServiceFactory
    {

    }
    class ProductServiceFactory : IProductServiceFactory
    {

    }
    public interface IProductService
    {
        void AddViewCount();
    }

    class ProductService : IProductService
    {
         private readonly Data.IProductRepository _Repository;
         private readonly IProductSearchManager _SearchManager;
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
            //搜索引擎
            var entityFromSearch = _SearchManager.SearchProductByPids(new[] { _LazyEntity.Value.pid }).FirstOrDefault();
            if (entityFromSearch != null)
            {
                if(entityFromSearch.view)
            }
            //每隔十次更新一下索引
        }
        private readonly Lazy<Data.Product> _LazyEntity;
    }
}
