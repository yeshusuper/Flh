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
    }
    public class ProductManager : IProductManager
    {
        private readonly Data.IProductRepository _Repository;
        private readonly IFileStore _FileStore;
        public ProductManager(Data.IProductRepository repository, IFileStore fileStore)
        {
            _Repository = repository;
            _FileStore = fileStore;
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
                var pids = products.Where(p => p.pid > 0).Select(p => p.pid).ToArray();
                var existsProducts = _Repository.EnabledProduct.Where(p => pids.Contains(p.pid)).ToArray();

                //更新已存在的产品
                foreach (var newProduct in existsProducts)
                {
                    var oldProduct = products.FirstOrDefault(p => p.pid == newProduct.pid);
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
                    if (oldProduct.imagePath != newProduct.imagePath)
                    {
                        var newFileID = FileId.FromFileId(newProduct.imagePath);
                        _FileStore.Copy(FileId.FromFileId(newProduct.imagePath), FileId.FromFileName(newProduct.imagePath));//将临时文件复制到永久文件处
                        oldProduct.imagePath = newFileID.Id;
                    }

                    OverrideIfNotNullNotWhiteSpace(oldProduct, newProduct, p => p.classNo, (p, v) => p.classNo = v);
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
                }

                //新增的产品  
                var addingProducts = products.Where(p => p.pid <= 0).ToArray();
                foreach (var entity in addingProducts)
                {
                    entity.created = DateTime.Now;
                    entity.updated = DateTime.Now;
                    entity.enabled = true;

                    var newFileID = FileId.FromFileId(entity.imagePath);
                    _FileStore.Copy(FileId.FromFileId(entity.imagePath), newFileID);//将临时文件复制到永久文件处
                    entity.imagePath = newFileID.Id;

                    _Repository.Add(entity);
                    _Repository.SaveChanges();
                    UpdateSearchIndex(entity.pid);  //更新索引
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
            if (args != null && (!String.IsNullOrWhiteSpace(args.ClassNo) || !String.IsNullOrWhiteSpace(args.Keyword)))
            {
                return ProductSearchHelper.Search(args, out count);
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
                _Repository.Update(p => pids.Contains(p.pid) && p.enabled, c => new Data.Product { enabled = false, updated = DateTime.Now, updater = uid });
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


        public IQueryable<Data.Product> AllProducts
        {
            get { return _Repository.Entities; }
        }
    }

    public class ProductSearchHelper
    {
        public static void UpdateSearchIndex(Data.Product entity)
        {
            if (entity != null)
            {
                AliyunHelper.UpdateIndexDoc(new ProductAliyunIndexer(), new Dictionary<string, object>[]{ new Dictionary<string, object>{ 
                {TableKey,entity.pid},
                {name,entity.name},
                {enname,entity.enName},
                {description,entity.description},
                {endescription,entity.enDescription},
                {size,entity.size},
                {ensize,entity.enSize},
                {color,entity.color},
                {encolor,entity.enColor},
                {material,entity.material},
                {enmaterial,entity.enMaterial},
                {technique,entity.technique},
                {entechnique,entity.enTechnique},
                {minquantity,entity.minQuantity},
                {deliveryday,entity.deliveryDay},
                {keywords,entity.keywords},
                {enkeywords,entity.enKeywords},
                {unitprice,entity.unitPrice},
                {imagepath,entity.imagePath},
                {classno,entity.classNo},
                {sortno,entity.sortNo},
                {createuid,entity.createUid},
                {created,entity.created},
                {updated,entity.updated},
                {enabled,entity.enabled},
                {updater,entity.updater??0},
                }
                });

            }
        }

        public static void DeleteIndex(params long[] pids)
        {
            AliyunHelper.DeleteIndexDoc(new ProductAliyunIndexer(), TableKey, pids.Select(p => p.ToString()).ToArray());
        }
        static String TableKey = "pid";

        public static Data.Product[] Search(ProductSearchArgs args, out int count)
        {
            var querys = new List<IQuery>();
            if (!String.IsNullOrWhiteSpace(args.Keyword))
            {
                querys.Add(Query.Or(
                     new QueryItem("keyword", args.Keyword)
                     , new QueryItem("enkeyword", args.Keyword)
                    ));
            }
            if (!String.IsNullOrWhiteSpace(args.ClassNo))
            {
                querys.Add(new KeywordQuery("classno", String.Format("^{0}", args.ClassNo)));
            }
            var query = new QueryBuilder
            {
                Config = new Config { Start = Math.Max(0, args.Start), Hit = Math.Max(1, args.Limit) },
                Query = Query.And(querys.ToArray()),
                // Sort = new SortItem("updated", SortKinds.Desc)
            };
            var result = AliyunHelper.Search(new ProductAliyunIndexer(), query, String.Empty);
            List<Data.Product> products = new List<Data.Product>();
            foreach (var item in result.Items)
            {
                products.Add(GetProduct(item));
            }
            count = result.Total;
            return products.ToArray();
        }

        //阿里云索引表结构字段
        static String name = "name";
        static String enname = "enname";
        static String description = "description";
        static String endescription = "endescription";
        static String size = "size";
        static String ensize = "ensize";
        static String color = "color";
        static String encolor = "encolor";
        static String material = "material";
        static String enmaterial = "enmaterial";
        static String technique = "technique";
        static String entechnique = "entechnique";
        static String minquantity = "minquantity";
        static String deliveryday = "deliveryday";
        static String keywords = "keywords";
        static String enkeywords = "enkeywords";
        static String unitprice = "unitprice";
        static String imagepath = "imagepath";
        static String classno = "classno";
        static String sortno = "sortno";
        static String createuid = "createuid";
        static String created = "created";
        static String updated = "updated";
        static String enabled = "enabled";
        static String updater = "updater";

        public static Data.Product GetProduct(Dictionary<string, string> dic)
        {
            Data.Product entity = new Data.Product();
            entity.pid = TryGetDictValue(dic, TableKey).To<long>();
            entity.name = TryGetDictValue(dic, name);
            entity.enName = TryGetDictValue(dic, enname);
            entity.description = TryGetDictValue(dic, description);
            entity.enDescription = TryGetDictValue(dic, endescription);
            entity.size = TryGetDictValue(dic, size);
            entity.enSize = TryGetDictValue(dic, ensize);
            entity.color = TryGetDictValue(dic, color);
            entity.enColor = TryGetDictValue(dic, encolor);
            entity.material = TryGetDictValue(dic, material);
            entity.enMaterial = TryGetDictValue(dic, enmaterial);
            entity.technique = TryGetDictValue(dic, technique);
            entity.enTechnique = TryGetDictValue(dic, entechnique);
            entity.minQuantity = TryGetDictValue(dic, minquantity).To<int>();
            entity.deliveryDay = TryGetDictValue(dic, deliveryday).To<int>();
            entity.keywords = TryGetDictValue(dic, keywords);
            entity.enKeywords = TryGetDictValue(dic, enkeywords);
            entity.unitPrice = TryGetDictValue(dic, unitprice).To<decimal>();
            entity.imagePath = TryGetDictValue(dic, imagepath);
            entity.classNo = TryGetDictValue(dic, classno);
            entity.sortNo = TryGetDictValue(dic, sortno).To<int>();
            entity.createUid = TryGetDictValue(dic, createuid).To<long>();
            entity.created = FieldHelper.ToDateTime(TryGetDictValue(dic, created).To<int>());
            entity.updated = FieldHelper.ToDateTime(TryGetDictValue(dic, updated).To<int>());
            entity.enabled = TryGetDictValue(dic, enabled).To<bool>();
            entity.updater = TryGetDictValue(dic, updater).To<long>();
            return entity;
        }

        static String TryGetDictValue(Dictionary<string, string> dic, String key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                return String.Empty;
            }
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
        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
