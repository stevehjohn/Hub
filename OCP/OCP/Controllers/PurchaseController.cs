using OCP.Data;
using OCP.Infrastructure;
using OCP.Models;
using System;
using System.Data;
using System.Web.Caching;
using System.Web.Mvc;

namespace OCP.Controllers
{
    public class PurchaseController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = GetModel();

            model.Quantity = "1";

            Utilities.LogEvent("PURCHASE_VIEW");

            return View(model);
        }

        [HttpPost]
        public ActionResult Validate(string email, string copies)
        {
            var model = GetModel();

            model.Email = email;
            model.Quantity = copies;

            model.InvalidEmail = !Utilities.IsValidEmail(model.Email);

            int qty;
            model.InvalidQuantity = !int.TryParse(model.Quantity, out qty);

            if (!model.InvalidQuantity && qty < 1)
            {
                model.InvalidQuantity = true;
            }

            if (!model.InvalidEmail && !model.InvalidQuantity)
            {
                var lienceGuid = GenerateNewLicence(email, qty);

                Utilities.LogEvent("PURCHASED");

                return RedirectToAction("licence", new { id = lienceGuid });
            }

            Utilities.LogEvent("PURCHASE_INVALID");

            return View("Index", model);
        }

        [HttpGet]
        public ActionResult Licence(Guid id)
        {
            var model = new LicenceModel();

            using (var db = new DB())
            {
                db.CreateCommand("LoadLicence");
                db.AddParameter("Guid", SqlDbType.UniqueIdentifier, id);

                var result = db.Execute();
                result.Read();

                model.NoOfLicences = (int)result["NoOfLicences"];
                model.LicenceCode = (string)result["Licence"];
            }

            return View(model);
        }

        private Guid GenerateNewLicence(string email, int copies)
        {
            var licenceGen = new LicenceGenerator();

            var licence = licenceGen.NewLicence(email, copies, new[] { "mongo" });

            var guid = Guid.NewGuid();

            using (var db = new DB())
            {
                db.StartTransaction();

                db.CreateCommand("UpsertUser");
                db.AddParameter("EmailAddress", SqlDbType.NVarChar, email);

                var result = db.Execute();
                result.Read();

                var userId = (long)result["UserId"];
                result.Close();

                db.CreateCommand("SaveLicence");
                db.AddParameter("UserId", SqlDbType.BigInt, userId);
                db.AddParameter("Licence", SqlDbType.VarChar, licence);
                db.AddParameter("Guid", SqlDbType.UniqueIdentifier, guid);
                db.AddParameter("NoOfLicences", SqlDbType.Int, copies);
                db.AddParameter("ValidModules", SqlDbType.VarChar, "mongo");

                long price = 0;
                if (copies < 5)
                {
                    price = copies * 5000;
                }
                else if (copies < 10)
                {
                    price = copies * 4750;
                }
                else
                {
                    price = copies * 45;
                }

                db.AddParameter("Price", SqlDbType.BigInt, price);

                db.ExecuteNonReader();

                db.CommitTransaction();
            }

            return guid;
        }

        private PurchaseModel GetModel()
        {
            var model = new PurchaseModel();

            model.SelectedPage = "purchase";

            ExchangeRates exch = null;
            var cacheObj = HttpContext.Cache["EXCH"];
            if (cacheObj != null)
            {
                exch = cacheObj as ExchangeRates;
            }
            if (exch == null)
            {
                exch = new ExchangeRates(50);
                if (exch.Succeeded)
                {
                    HttpContext.Cache.Add("EXCH", exch, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0), CacheItemPriority.NotRemovable, null);
                }
            }

            model.ExchangeRates = exch;

            return model;
        }
    }
}
