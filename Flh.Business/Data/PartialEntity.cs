using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Business.Data
{
    public partial class Admin
    {
        public static Admin CreateNewInstance(long uid)
        {
            return new Admin { uid = uid, enabled = true, created = DateTime.Now };
        }
    }

    public interface IProduct
    {
        long pid { get; }
        string name { get; }
        string enName { get; }
        string description { get; }
        string enDescription { get; }
        string size { get; }
        string enSize { get; }
        string color { get; }
        string enColor { get; }
        string material { get; }
        string enMaterial { get; }
        string technique { get; }
        string enTechnique { get; }
        int minQuantity { get; }
        int deliveryDay { get; }
        string keywords { get; }
        string enKeywords { get; }
        decimal unitPrice { get; }
        string imagePath { get; }
        string classNo { get; }
        int sortNo { get; }
        long createUid { get; }
        System.DateTime created { get; }
        System.DateTime updated { get; }
        bool enabled { get; }
        Nullable<long> updater { get; }
        Nullable<int> viewCount { get; }
    }
    public partial class Product : IProduct
    {
    }
}
