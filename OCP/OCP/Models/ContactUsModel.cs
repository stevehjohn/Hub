using OCP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCP.Models
{
    public class ContactUsModel : ModelBase
    {
        public string Email { get; set; }

        public bool InvalidEmail { get; set; }

        public string MessageType { get; set; }

        public string Message { get; set; }

        public bool InvalidMessage { get; set; }

        public bool Sent { get; set; }

        public void SaveAndSendMessage()
        {
            try
            {
                using (DB db = new DB())
                {
                    db.CreateCommand("SaveMessage");

                    db.AddParameter("EmailAddress", System.Data.SqlDbType.NVarChar, Email);
                    db.AddParameter("MessageType", System.Data.SqlDbType.NVarChar, MessageType);
                    db.AddParameter("Message", System.Data.SqlDbType.NVarChar, Message);

                    db.ExecuteNonReader();

                    db.CommitTransaction();
                }
            }
            catch { }
        }
    }
}