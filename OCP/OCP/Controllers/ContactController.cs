using OCP.Infrastructure;
using OCP.Models;
using System.Web.Mvc;

namespace OCP.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new ContactUsModel();

            model.SelectedPage = "contact";

            Utilities.LogEvent("CONTACT_VIEW");

            return View(model);
        }

        public ActionResult Validate(string email, string messageType, string message)
        {
            var model = new ContactUsModel();

            model.SelectedPage = "contact";

            model.Email = email;
            model.MessageType = messageType;
            model.Message = message;

            model.InvalidEmail = !Utilities.IsValidEmail(model.Email);

            model.InvalidMessage = string.IsNullOrWhiteSpace(model.Message);

            if (!model.InvalidEmail && !model.InvalidMessage)
            {
                model.SaveAndSendMessage();
                model.Sent = true;

                Utilities.LogEvent("FEEDBACK_SENT");
            }
            else
            {
                Utilities.LogEvent("CONTACT_INVALID");
            }

            return View("Index", model);
        }
    }
}
