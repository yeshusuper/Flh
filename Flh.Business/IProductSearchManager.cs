using Flh.Aliyun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business
{
    public interface IProductSearchManager
    {
        Data.Product[] Search(ProductSearchArgs args, out int count);
        Data.Product[] SearchProductByPids(long[] pids);
        void DeleteIndex(params long[] pids);
        void UpdateSearchIndex(Data.Product entity);
    }
    public class ProductSearchManager:IProductSearchManager
    {
        public Data.Product[] Search(ProductSearchArgs args, out int count)
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

            if (args.Pids != null && args.Pids.Any())
            {
                List<QueryItem> queryItems = new List<QueryItem>();
                foreach (var pid in args.Pids)
                {
                    queryItems.Add(new QueryItem("pid", pid.ToString()));
                }
                querys.Add(Query.Or(queryItems.ToArray()));
            }

            //排序策略
            var sort = new SourtItemCollection(new SortItem(sortno, SortKinds.Desc), new SortItem(updated, SortKinds.Desc));//默认排序
            if (args.Sort == null)
            {
                if (args.Sort == SortType.PriceAsc)//价格排序
                {
                    sort = new SourtItemCollection(new SortItem(unitprice, SortKinds.Asc));
                }
                else if (args.Sort == SortType.TimeDesc)//更新时间排序
                {
                    sort = new SourtItemCollection(new SortItem(updated, SortKinds.Desc));
                }
                else if (args.Sort == SortType.ViewDesc)//查看量排序
                {
                    sort = new SourtItemCollection(new SortItem(viewcount, SortKinds.Desc));
                }
            }

            var query = new QueryBuilder
            {
                Config = new Config { Start = Math.Max(0, args.Start), Hit = Math.Max(1, args.Limit) },
                Query = Query.And(querys.ToArray()),
                Sort = sort,
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

        public void DeleteIndex(params long[] pids)
        {
            AliyunHelper.DeleteIndexDoc(new ProductAliyunIndexer(), TableKey, pids.Select(p => p.ToString()).ToArray());
        }

        public void UpdateSearchIndex(Data.Product entity)
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
                {viewcount,entity.viewCount??0},
                }
                });
            }
        }

        public Data.Product[] SearchProductByPids(long[] pids)
        {
            ExceptionHelper.ThrowIfNull(pids,"pids");
            int count;
            return Search(new ProductSearchArgs { Pids = pids }, out count);
        }

        #region 辅助方法
        static Data.Product GetProduct(Dictionary<string, string> dic)
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
            entity.viewCount = TryGetDictValue(dic, viewcount).To<int>();
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

        static String TableKey = "pid";

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
        static String viewcount = "viewcount"; 
        #endregion
    }
}
