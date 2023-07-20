using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPrinter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICommandBus _commandBus;
        public HomeController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public ActionResult Index()
        {
            var re = _commandBus.SendAsync(new GetPreparationOrdersQuery()), 

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}