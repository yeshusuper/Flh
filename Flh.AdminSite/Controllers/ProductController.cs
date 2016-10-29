using Flh.Business;
using Flh.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Flh.AdminSite.Controllers
{
     [FlhAuthorize]
    public class ProductController : BaseController
    {
       private readonly IProductManager _ProductManager;
       public ProductController(IProductManager productManager)
       {
           _ProductManager = productManager;
       }
        public ActionResult List()
        {
            return View();
        }

    }
}
