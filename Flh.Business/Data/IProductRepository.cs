using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IProductRepository : Flh.Data.IRepository<Product>
    {
        IQueryable<Product> EnabledProduct { get; }
    }
    internal class ProductRepository : DbSetRepository<FlhContext, Product>, IProductRepository
    {
        public ProductRepository(FlhContext context) : base(context) { }
        public IQueryable<Product> EnabledProduct
        {
            get { return Context.Product.Where(d=>d.enabled==true); }
        }

        //public void Add(Data.Product entity)
        //{
        //    entity.created = DateTime.Now;
        //    entity.enabled = true;
        //    base.Add(entity);
        //}
    }
}
