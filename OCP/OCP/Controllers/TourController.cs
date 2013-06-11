using OCP.Infrastructure;
using OCP.Models;
using System.Web.Mvc;

namespace OCP.Controllers
{
    public class TourController : Controller
    {
        public ActionResult Page(int pageNo)
        {
            if (pageNo < 1)
            {
                pageNo = 1;
            }
            else if (pageNo > 7)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new TourModel();

            model.SelectedPage = string.Empty;
            model.HideHeadline = true;
            model.PageNo = pageNo;

            Utilities.LogEvent(string.Format("TOUR_{0}_VIEW", pageNo));

            return View(model);
        }

    }
}
