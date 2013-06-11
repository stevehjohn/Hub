using OCP.Data;
using System;
using System.Data;

namespace OCP.ApiModel
{
    public class TrialInfo
    {
        public Guid Guid { get; set; }

        public DateTime UtcDate { get; set; }

        public DateTime LocalDate { get; set; }

        public string Event { get; set; }

        public void Save() 
        {
            try
            {
                using (var db = new DB())
                {
                    db.CreateCommand("SaveTrialInfo");

                    db.AddParameter("Guid", SqlDbType.UniqueIdentifier, Guid);
                    db.AddParameter("UtcDate", SqlDbType.DateTime, UtcDate);
                    db.AddParameter("LocalDate", SqlDbType.DateTime, LocalDate);
                    db.AddParameter("Event", SqlDbType.VarChar, Event);

                    db.ExecuteNonReader();

                    db.CommitTransaction();
                }
            }
            catch { }
        }
    }
}