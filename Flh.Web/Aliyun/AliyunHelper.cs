//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Flh.Web.Aliyun
//{
    //public class AliyunHelper
    //{
        //public readonly static AliyunAccessKey AliyunAccessKey;

        //static AliyunHelper() {
        //    var value = new System.Configuration.AppSettingsReader().GetValue("aliyunAccessKey", typeof(String)) as string;
        //    if (value != null)
        //    {
        //        var arr = value.Split(new []{",", ";"}, StringSplitOptions.RemoveEmptyEntries);
        //        if (arr.Length > 1)
        //        {
        //            string id = arr[0], secret = arr[1];
        //            if (!String.IsNullOrWhiteSpace(id) && !String.IsNullOrWhiteSpace(secret))
        //            {
        //                AliyunAccessKey = new AliyunAccessKey(id.Trim(), secret.Trim());
        //            }
        //        }
        //    }
        //}
    //}
//}
