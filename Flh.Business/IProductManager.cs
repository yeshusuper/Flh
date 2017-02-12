using Flh.Aliyun;
using Flh.IO;
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
        IQueryable<Data.Product> AllProducts { get; }
        void Delete(long uid, long[] pids);
        Data.ProductWithRowNoItem[] GetProjectsWithRowNumber(Data.ProductWithRowNoItemArgs args);
    }
    public class ProductManager : IProductManager
    {
        private readonly Data.IProductRepository _Repository;
        private readonly IFileStore _FileStore;
        private readonly IProductSearchManager _SearchManager;
        public ProductManager(Data.IProductRepository repository, 
            IFileStore fileStore,
            IProductSearchManager searchManager)
        {
            _Repository = repository;
            _FileStore = fileStore;
            _SearchManager = searchManager;
        }

        void VeriryEntity(Data.Product newProduct)
        {
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.name, "", "产品名称不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.description, "", "产品详细说明不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.size, "", "产品尺寸不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.color, "", "产品颜色不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.material, "", "产品材质不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.technique, "", "产品工艺不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.keywords, "", "产品关键词不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.imagePath, "", "产品图片不能为空");
            ExceptionHelper.ThrowIfNullOrWhiteSpace(newProduct.classNo, "", "产品分类编号不能为空");
            ExceptionHelper.ThrowIfTrue(newProduct.minQuantity <= 0, "", "产品起订量必须大于0");
            ExceptionHelper.ThrowIfTrue(newProduct.deliveryDay < 0, "", "产品发货日必须大于或等于0");
            ExceptionHelper.ThrowIfTrue(newProduct.unitPrice <= 0, "", "产品单价必须大于0");
            ExceptionHelper.ThrowIfTrue(newProduct.updater <= 0, "", "没有传入更新者的userID");
        }

        public void AddOrUpdateProducts(Data.Product[] products)
        {
            ExceptionHelper.ThrowIfNull(products, "products");
            if (products.Any())
            {
                foreach (var item in products)
                {
                    VeriryEntity(item);
                }

                //更新已存在的产品
                var updatingProducts = products.Where(p => p.pid > 0).ToArray();
                foreach (var newProduct in updatingProducts)
                {
                    var oldProduct = AllProducts.FirstOrDefault(p => p.pid == newProduct.pid);
                    if (oldProduct != null)
                    {
                        using (var scope = new System.Transactions.TransactionScope())
                        {
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
                            OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.classNo, (p, v) => p.classNo = v);

                            var newProductFileId = FileId.FromFileId(newProduct.imagePath);
                            var newFileID = newProductFileId.ToStorageId();
                            var imgChange = oldProduct.imagePath != newProduct.imagePath && oldProduct.imagePath != newFileID.Id;
                            if (imgChange)
                                oldProduct.imagePath = newFileID.Id;
                            oldProduct.minQuantity = newProduct.minQuantity;
                            oldProduct.deliveryDay = newProduct.deliveryDay;
                            oldProduct.unitPrice = newProduct.unitPrice;
                            oldProduct.sortNo = newProduct.sortNo;
                            oldProduct.updated = DateTime.Now;
                            if (newProduct.updater > 0)
                            {
                                oldProduct.updater = newProduct.updater;
                            }
                            _Repository.SaveChanges();
                            UpdateSearchIndex(oldProduct.pid);//更新索引
                            if (newProductFileId.IsTempId)
                                _FileStore.Copy(newProductFileId, newFileID);//将临时文件复制到永久文件处                     
                            scope.Complete();
                        }
                        System.Threading.Thread.Sleep(300);//同时添加太多产品，搜索引擎更新太频繁会报错（阿里云限制每秒频率，除非加钱）,这里添加一个产品后先休眠几百毫秒
                    }
                }

                //新增的产品
                var addingProducts = products.Where(p => p.pid <= 0).ToArray();
                foreach (var entity in addingProducts)
                {
                    using (var scope = new System.Transactions.TransactionScope())
                    {
                        entity.created = DateTime.Now;
                        entity.updated = DateTime.Now;
                        entity.enabled = true;

                        var temFileId = FileId.FromFileId(entity.imagePath);
                        var newFileID = temFileId.ToStorageId();
                        entity.imagePath = newFileID.Id;
                        _Repository.Add(entity);
                        _Repository.SaveChanges();
                        if (temFileId.IsTempId)
                            _FileStore.Copy(temFileId, newFileID);//将临时文件复制到永久文件处
                        UpdateSearchIndex(entity.pid);  //更新索引
                        scope.Complete();
                    }
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

        /// <summary>
        /// 如果新值不为空就覆盖旧的值
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        /// <param name="newValue"></param>
        /// <param name="setValue"></param>
        private void OverrideIfNotNullNotWhiteSpace(Data.Product oldEntity, Data.Product newEntity, Func<Data.Product, String> newValue, Action<Data.Product, String> setValue)
        {
            if (!String.IsNullOrWhiteSpace(newValue(newEntity)))
            {
                setValue(oldEntity, newValue(newEntity));
            }
        }
        public IEnumerable<Data.Product> Search(ProductSearchArgs args, out int count)
        {
            if (args != null
                && (!String.IsNullOrWhiteSpace(args.Keyword) || !String.IsNullOrWhiteSpace(args.Color))
                )
            {
                args.Keyword = ((args.Keyword ?? String.Empty) +" "+ args.Color).Trim();
                return _SearchManager.Search(args, out count);
            }
            else
            {
                int start = 0;
                int limit = 30;
                var source = _Repository.EnabledProduct;
                if (args != null)
                {
                    if (!String.IsNullOrWhiteSpace(args.ClassNo))
                    {
                        source = source.Where(d => d.classNo.StartsWith(args.ClassNo.Trim()));
                    }
                    if (!String.IsNullOrWhiteSpace(args.Keyword))
                    {
                        var keyword = args.Keyword.Trim();
                        source = source.Where(d => d.name.Contains(keyword) || d.keywords.Contains(keyword));
                    }
                    if (!String.IsNullOrWhiteSpace(args.Color))
                    {
                        source = source.Where(d=>d.color.Contains(args.Color));
                    }
                    if (args.PriceMin.HasValue)
                    {
                        source = source.Where(d => d.unitPrice >= args.PriceMin.Value);
                    }
                    if (args.PriceMax.HasValue)
                    {
                        source = source.Where(d => d.unitPrice <= args.PriceMax.Value);
                    }
                    start = Math.Max(0, args.Start);
                    if (args.Limit > 0)
                        limit = args.Limit;
                }
                count = source.Count();
                if (args.Sort == null)
                {
                    source = source.OrderByDescending(p => p.sortNo)
                   .ThenByDescending(p => p.updated);
                }
                else if (args.Sort == SortType.TimeDesc)
                {
                    source = source.OrderByDescending(p => p.updated);
                }
                else if (args.Sort == SortType.PriceAsc)
                {
                    source = source.OrderBy(p => p.unitPrice);
                }
                else if (args.Sort == SortType.ViewDesc)
                {
                    source = source.OrderByDescending(p => p.viewCount);
                }
                return source.Skip(start).Take(limit).ToArray();
            }

        }
        public IQueryable<Data.Product> EnabledProducts
        {
            get { return _Repository.EnabledProduct; }
        }
        public void Delete(long uid, long[] pids)
        {
            ExceptionHelper.ThrowIfNotId(uid, "uid");
            pids = (pids ?? Enumerable.Empty<long>()).Where(id => id > 0).Distinct().ToArray();
            if (pids.Length > 0)
            {
                using (var scope = new System.Transactions.TransactionScope())
                {
                    _Repository.Update(p => pids.Contains(p.pid) && p.enabled, c => new Data.Product { enabled = false, updated = DateTime.Now, updater = uid });
                     _SearchManager.DeleteIndex(pids);//删除索引
                    scope.Complete();
                }
            }
        }

        void UpdateSearchIndex(long pid)
        {
            var entity = _Repository.EnabledProduct.FirstOrDefault(d => d.pid == pid);
            if (entity != null)
            {
                _SearchManager.UpdateSearchIndex(entity);
            }
        }


        public IQueryable<Data.Product> AllProducts
        {
            get { return _Repository.Entities; }
        }

        public Data.ProductWithRowNoItem[] GetProjectsWithRowNumber(Data.ProductWithRowNoItemArgs args)
        {
            return _Repository.GetProjectsWithRowNumber(args);
        }
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
        public string Keyword { get; set; }
        public long[] Pids { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
        public SortType? Sort { get; set; }
        public String Color { get; set; }
    }

    public enum SortType
    {
        TimeDesc = 0,
        ViewDesc = 1,
        PriceAsc = 2,
    }
}
