using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCP.Models
{
    public class PurchaseModel : ModelBase
    {
        public ExchangeRates ExchangeRates { get; set; }

        public string Email { get; set; }

        public bool InvalidEmail { get; set; }

        public string Quantity { get; set; }

        public bool InvalidQuantity { get; set; }
    }
}