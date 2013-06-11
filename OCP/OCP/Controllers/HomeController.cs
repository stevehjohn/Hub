using OCP.Infrastructure;
using OCP.Models;
using System.Web.Mvc;

namespace OCP.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new ModelBase();

            model.SelectedPage = "software";

            Utilities.LogEvent("HOME_VIEW");

            return View(model);
        }

        public ActionResult Download()
        {
            var model = new ModelBase();

            model.SelectedPage = "software";

            Utilities.LogEvent("DOWNLOAD");

            return Redirect("/Downloads/Ming.application");
        }
    }
}
