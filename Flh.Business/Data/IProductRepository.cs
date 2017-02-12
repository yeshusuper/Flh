using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public interface IProductRepository : Flh.Data.IRepository<Product>
    {
        IQueryable<Product> EnabledProduct { get; }
        ProductWithRowNoItem[] GetProjectsWithRowNumber(ProductWithRowNoItemArgs args);
    }
  
    internal class ProductRepository : DbSetRepository<FlhContext, Product>, IProductRepository
    {
        public ProductRepository(FlhContext context) : base(context) { }
        public IQueryable<Product> EnabledProduct
        {
            get { return Context.Product.Where(d=>d.enabled==true); }
        }

        public ProductWithRowNoItem[] GetProjectsWithRowNumber(ProductWithRowNoItemArgs args)
        {
            List<SqlParameter> ps = new List<SqlParameter>();
            StringBuilder sb = new StringBuilder();
            sb.Append(@"SELECT * FROM(
SELECT [pid]
      ,[name]      
      ,[imagePath]
	  ,ROW_NUMBER() OVER(order by sortNo DESC,updated DESC) rno
  FROM [dbo].[Product] p where 1=1 and enabled=1");
            if (!String.IsNullOrWhiteSpace(args.ClassNo))
            {
                sb.Append(" and p.classNo like @classNo");
                ps.Add(new SqlParameter("classNo",args.ClassNo+"%"));
            }
            sb.Append(" ) AS tb where 1=1 ");
            if (args.MinRno.HasValue)
            {
                sb.Append(" and rno>=@MinRno");
                ps.Add(new SqlParameter("MinRno", args.MinRno.Value));
            }
            if (args.MaxRno.HasValue)
            {
                sb.Append(" and rno<=@MaxRno");
                ps.Add(new SqlParameter("MaxRno", args.MaxRno.Value));
            }
            if (args.Pid.HasValue)
            {
                sb.Append("  and pid=@pid");
                ps.Add(new SqlParameter("pid",args.Pid.Value));
            }
            return Context.Database.SqlQuery<ProductWithRowNoItem>(sb.ToString(), ps.ToArray()).ToArray();
        }
        //public void Add(Data.Product entity)
        //{
        //    entity.created = DateTime.Now;
        //    entity.enabled = true;
        //    base.Add(entity);
        //}
    }

      public class ProductWithRowNoItem
    {
        public long pid{get;set;}
        public long rno{get;set;}
        public String name{get;set;}
        public String imagePath{get;set;}

    }
    public class ProductWithRowNoItemArgs{
        public long? MinRno { get; set; }
        public long? MaxRno { get; set; }
        public long? Pid{get;set;}
        public String ClassNo{get;set;}
    }
}
