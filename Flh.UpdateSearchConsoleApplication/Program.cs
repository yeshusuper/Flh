using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.IO;

namespace Flh.UpdateSearchConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n\n正在准备更新索引...");
            var fileName = "updateIndexMaxPid.txt";
            var kernel = new Ninject.StandardKernel(new Flh.Business.Inject.DataModule(),new Flh.Business.Inject.ServiceModule());
            var productManager = kernel.Get<Flh.Business.IProductManager>();           
            var productArgs = new Business.ProductListArgs();
            
            //获取上次的索引更新的进度
            if (File.Exists(fileName))
            {
                productArgs.MinPid = long.Parse(File.ReadAllText(fileName));
            }
            else
            {
                productArgs.MinPid = 0;
            }

            //查出还没更新索引的产品
            var products = productManager.GetProductList(productArgs).OrderBy(d=>d.pid).ToArray();
            if (products.Any())
            {
                foreach (var product in products)
                {
                    productManager.UpdateSearchIndex(product.pid);
                    File.WriteAllText(fileName, product.pid.ToString());//保存索引进度
                    Console.WriteLine("正在更新索引：" + product.name + " " + product.pid);
                }
            }
            else
            {               
                File.WriteAllText(fileName, "0"); //清空索引进度
                Console.WriteLine("搜索引擎更新完毕！按回车键退出");
            }
           
            Console.Read();
        }
    }
}
