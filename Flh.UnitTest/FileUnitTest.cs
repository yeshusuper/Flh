using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flh.IO;
using System.IO;

namespace Flh.UnitTest
{
    [TestClass]
    public class FileUnitTest
    {
        [TestMethod]
        public void TestFileCreateOrUpdate()
        {
            var manager = new FileManager(new SystemFileStroe());
            var target = FileId.FromFileId("2.jpg");
            using (var fs = File.OpenRead("1.jpg"))
            {
                manager.CreateOrUpdate(target, fs);
            }
            Assert.IsTrue(manager.Exists(target));
            manager.Delete(target);
        }
    }
}
