using Flh.Aliyun;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Flh.Business
{
    public interface IProductManager
    {
        void AddOrUpdateProducts(Data.Product[] products);
        IEnumerable<Data.Product> Search(ProductSearchArgs args, out int count);
        IQueryable<Data.Product> GetProductList(ProductListArgs args);
        IQueryable<Data.Product> EnabledProducts { get; }
        void Delete(long uid, long[] pids);
    }
    public class ProductManager : IProductManager
    {
        private readonly Data.IProductRepository _Repository;
        public ProductManager(Data.IProductRepository repository)
        {
            _Repository = repository;
        }

        public void AddOrUpdateProducts(Data.Product[] products)
        {
            ExceptionHelper.ThrowIfNull(products, "products");
            if (products.Any())
            {
                List<long> searchIndexPids = new List<long>();
                var pids = products.Where(p => p.pid > 0).Select(p => p.pid).ToArray();
                var existsProducts = _Repository.EnabledProduct.Where(p => pids.Contains(p.pid)).ToArray();
                var addingProducts = products.Where(p => p.pid <= 0).ToArray();
                using (var scope = new System.Transactions.TransactionScope())
                {
                    //更新已存在的产品
                foreach (var oldProduct in existsProducts)
                {
                    var newProduct = products.FirstOrDefault(p => p.pid == oldProduct.pid);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.name, (p, v) => p.name = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enName, (p, v) => p.enName = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.description, (p, v) => p.description = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enDescription, (p, v) => p.enDescription = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.size, (p, v) => p.size = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enSize, (p, v) => p.enSize = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.color, (p, v) => p.color = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enColor, (p, v) => p.enColor = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.material, (p, v) => p.material = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enMaterial, (p, v) => p.enMaterial = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.technique, (p, v) => p.technique = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enTechnique, (p, v) => p.enTechnique = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.keywords, (p, v) => p.keywords = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.enKeywords, (p, v) => p.enKeywords = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.imagePath, (p, v) => p.imagePath = v);
                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.classNo, (p, v) => p.classNo = v);
                    oldProduct.minQuantity = newProduct.minQuantity;
                    oldProduct.deliveryDay = newProduct.deliveryDay;
                    oldProduct.unitPrice = newProduct.unitPrice;
                    oldProduct.sortNo = newProduct.sortNo;
                    oldProduct.updated = DateTime.Now;
                        searchIndexPids.Add(oldProduct.pid);
                }

                    //新增的产品
                foreach (var item in addingProducts)
                {
                    AddEnabledProduct(item);
                        searchIndexPids.Add(item.pid);
                }
                _Repository.SaveChanges();
                    scope.Complete();
                }

                //重新更新索引
                foreach (var item in existsProducts)
                {
                    UpdateSearchIndex(item.pid);
            }
                foreach (var item in addingProducts)
                {
                    UpdateSearchIndex(item.pid);
                }
            }
        }

        public IQueryable<Data.Product> GetProductList(ProductListArgs args)
        {
            ExceptionHelper.ThrowIfNull(args, "args");
            var query = _Repository.EnabledProduct;
            if (args != null)
            {
            if (!String.IsNullOrWhiteSpace(args.ClassNo))
            {
                query = query.Where(d => d.classNo.StartsWith(args.ClassNo));
            }
            if (args.Pids != null && args.Pids.Any())
            {
                query = query.Where(d => args.Pids.Contains(d.pid));
            }
                if (args.MinPid > 0)
                {
                    query = query.Where(d => d.pid > args.MinPid);
                }
            }
            return query;
        }

        private void AddEnabledProduct(Data.Product entity)
        {
            entity.created = DateTime.Now;
            entity.enabled = true;
            _Repository.Add(entity);
        }

        private void OverrideIfNotNullNotWhiteSpace(Data.Product oldEntity, Data.Product newEntity, Func<Data.Product, String> newValue, Action<Data.Product, String> setValue)
        {
            if (!String.IsNullOrWhiteSpace(newValue(newEntity)))
            {
                setValue(oldEntity, newValue(newEntity));
            }
        }
        public IEnumerable<Data.Product> Search(ProductSearchArgs args,out int count)
        {
            int start = 0;
            int limit = 30;
            var source = _Repository.EnabledProduct;
            if (args!=null)
            {
                if (!String.IsNullOrWhiteSpace(args.ClassNo))
                {
                    source = source.Where(d => d.classNo.StartsWith(args.ClassNo.Trim()));
                }
                if (!String.IsNullOrWhiteSpace(args.Keyword))
                {
                var keyword=args.Keyword.Trim();
                    source = source.Where(d =>d.name.Contains(keyword)||d.keywords.Contains(keyword));
                }
                start = Math.Max(0, args.Start);
                if (args.Limit > 0)
                    limit = args.Limit;
            }
            count = source.Count();
            return source.OrderByDescending(p => p.sortNo)
                 .ThenByDescending(p => p.updated)
                 .Skip(start).Take(limit)
                 .ToArray();
        }
        public IQueryable<Data.Product> EnabledProducts
        {
            get { return _Repository.EnabledProduct; }
        }
        public void Delete(long uid, long[] pids)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            pids = (pids ?? Enumerable.Empty<long>()).Where(id=>id>0).Distinct().ToArray();
            if (pids.Length > 0)
            {
                _Repository.Update(p => pids.Contains(p.pid) &&p.enabled, c => new Data.Product { enabled = false, updated = DateTime.Now, updater = uid });
                ProductSearchHelper.DeleteIndex(pids);//删除索引
            }
        }

        void UpdateSearchIndex(long pid)
        {
            var entity = _Repository.EnabledProduct.FirstOrDefault(d => d.pid == pid);
            if (entity != null)
            {
                ProductSearchHelper.UpdateSearchIndex(entity);
            }
        }
    }

    public class ProductSearchHelper
    {
        public static void UpdateSearchIndex(Data.Product entity)
        {
            if (entity != null)
            {
                AliyunHelper.UpdateIndexDoc(AliyunConfig.AccessKeyId, AliyunConfig.AccessKeySecret, new ProductAliyunIndexer(), new Dictionary<string, object>[]{ new Dictionary<string, object>{ 
                {TableKey,entity.pid},
                {"name",entity.name},
                {"enname",entity.enName},
                {"description",entity.description},
                {"endescription",entity.enDescription},
                {"size",entity.size},
                {"ensize",entity.enSize},
                {"color",entity.color},
                {"encolor",entity.enColor},
                {"material",entity.material},
                {"enmaterial",entity.enMaterial},
                {"technique",entity.technique},
                {"entechnique",entity.enTechnique},
                {"minquantity",entity.minQuantity},
                {"deliveryday",entity.deliveryDay},
                {"keywords",entity.keywords},
                {"enKeywords",entity.enKeywords},
                {"unitprice",entity.unitPrice},
                {"imagepath",entity.imagePath},
                {"classno",entity.classNo},
                {"sortno",entity.sortNo},
                {"createUid",entity.createUid},
                {"created",entity.created},
                {"updated",entity.updated},
                {"enabled",entity.enabled},
                {"updater",entity.updater},
                }
                });

            }
        }

        public static void DeleteIndex(params long[] pids)
        {
            AliyunHelper.DeleteIndexDoc(AliyunConfig.AccessKeyId, AliyunConfig.AccessKeySecret, new ProductAliyunIndexer(), TableKey, pids.Select(p => p.ToString()).ToArray());
        }
        static String TableKey = "pid";
    }
    public class ProductListArgs
    {
        public ProductListArgs()
        {
            //ClassNoes=new String[0];
            Pids = new long[0];
        }
        public String ClassNo { get; set; }
        public long[] Pids { get; set; }
        public long MinPid { get; set; }
    }
    public class ProductSearchArgs
    {
        public String ClassNo { get; set; }
        public string  Keyword { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
