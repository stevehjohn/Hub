using OCP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCP.Infrastructure
{
    public static class Utilities
    {
        private const string localchars = "abcdefghijklmnopqrstuvwxyz0123456789!#$%&'*+-/=?^_`{|}~.";
        private const string domainchars = "abcdefghijklmnopqrstuvwxyz0123456789-.";

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            email = email.ToLower();

            if (email.Contains(".."))
                return false;

            if (email[0] == '.')
                return false;

            if (email[email.Length - 1] == '.')
                return false;

            int at = email.IndexOf("@");
            if (at < 0)
                return false;

            string local = email.Substring(0, at);
            string domain = email.Substring(at + 1);

            if (string.IsNullOrEmpty(local))
                return false;

            if (string.IsNullOrEmpty(domain))
                return false;

            if (!domain.Contains("."))
                return false;

            if (ContainsOtherChars(localchars, local))
                return false;

            if (ContainsOtherChars(domainchars, domain))
                return false;

            return true;
        }

        public static bool ContainsOtherChars(string acceptedChars, string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            for (int sidx = 0; sidx < str.Length; sidx++)
            {
                bool bFound = false;
                for (int acidx = 0; acidx < acceptedChars.Length; acidx++)
                {
                    if (str[sidx] == acceptedChars[acidx])
                    {
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                    return false;
            }

            return false;
        }

        public static void LogEvent(string eventToken)
        {
            Task.Factory.StartNew(() => DoLogEvent(eventToken));
        }

        private static void DoLogEvent(string eventToken)
        {
            try
            {
                using (DB db = new DB())
                {
                    db.CreateCommand("SaveEvent");

                    db.AddParameter("Event", System.Data.SqlDbType.VarChar, eventToken);

                    db.ExecuteNonReader();

                    db.CommitTransaction();
                }
            }
            catch { }
        }
    }
}