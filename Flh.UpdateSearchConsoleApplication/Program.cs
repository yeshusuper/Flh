using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using System.IO;
using Flh.Business;

namespace Flh.UpdateSearchConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n\n正在准备更新索引...");
            var fileName = "updateIndexMaxPid.txt";
            var kernel = new Ninject.StandardKernel(
                new Flh.Business.Inject.DataModule()
                , new Flh.Business.Inject.ServiceModule()
                ,new FileModule());
            var productManager = kernel.Get<Flh.Business.IProductManager>();
           
            long maxPid = 0;
            var maxEntity = productManager.AllProducts.OrderByDescending(p => p.pid).FirstOrDefault();
            if (maxEntity != null)
            {
                maxPid = maxEntity.pid;
            }

            while (true)
            {                
                //获取上次的索引更新的进度
                long minPid=0;
                if (File.Exists(fileName))
                {
                    minPid = long.Parse(File.ReadAllText(fileName));
                }
                else
                {
                    minPid = 0;
                }
                if (minPid > 0)
                {
                    Console.WriteLine("\n接着上次的进度，从" + minPid + "开始更新");
                }
                else
                {
                    Console.WriteLine("\n从零开始更新索引");
                }
                
                //查出还没更新索引的产品
                var products = productManager.AllProducts.Where(d => d.pid > minPid)
                    .OrderBy(d => d.pid)
                    .Take(30).ToArray();                
                if (products.Any())
                {
                    foreach (var product in products)
                    {
                        if (product.enabled)
                        {
                            ProductSearchHelper.UpdateSearchIndex(product);//更新索引
                        }
                        else
                        {
                            ProductSearchHelper.DeleteIndex(product.pid);//删除索引
                        }
                        File.WriteAllText(fileName, product.pid.ToString());//保存索引进度
                        Console.WriteLine("正在更新索引：" + product.pid + "/" + maxPid + " " + product.name);
                    }
                }
                else
                {
                    File.WriteAllText(fileName, "0"); //清空索引进度
                    Console.WriteLine("\n搜索引擎更新完毕！按回车键退出");
                    break;
                }
            }
            Console.Read();
        }

        private class FileModule : Ninject.Modules.NinjectModule
        {
            public override void Load()
            {
                Bind<Flh.IO.FileManager>().ToSelf();
                Bind<Flh.IO.IFileStore>().To<Flh.IO.SystemFileStroe>();
            }
        }
    }
}
