using OCP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace OCP.Models
{
    public class ExchangeRates
    {
        private double _costGbp;

        private double _gbpBase;

        private double _usd;

        private double _aud;

        private double _eur;

        private double _cad;

        private DateTime _date;

        private bool _success = false;

        public string Usd
        {
            get
            {
                return ShowExchangeRateOrError("${0} US", _usd);
            }
        }

        public string Aud
        {
            get
            {
                return ShowExchangeRateOrError("${0} Australian", _aud);
            }
        }

        public string Eur
        {
            get
            {
                return ShowExchangeRateOrError("€{0}", _eur);
            }
        }

        public string Cad
        {
            get
            {
                return ShowExchangeRateOrError("${0} Canadian", _cad);
            }
        }

        public string Date
        {
            get
            {
                return string.Format("{0:d MMM}", _date);
            }
        }

        public bool Succeeded
        {
            get
            {
                return _success;
            }
        }
        
        public ExchangeRates(double costGbp)
        {
            _costGbp = costGbp;
            LoadExchangeRates();
        }

        private string ShowExchangeRateOrError(string formatString, double amount)
        {
            if (amount == 0)
            {
                return string.Format(formatString, @"unavailable");
            }
            return string.Format(string.Format(formatString, "{0:0.00}"), amount);
        }

        private void LoadExchangeRatesFromDatabase()
        {
            try
            {
                using (DB db = new DB())
                {
                    db.CreateCommand("LoadCurrentExchangeRates");

                    var reader = db.Execute();

                    if (reader.Read())
                    {
                        _aud = (double)reader["AUD"];
                        _cad = (double)reader["CAD"];
                        _eur = (double)reader["EUR"];
                        _usd = (double)reader["USD"];
                        _date = (DateTime)reader["Date"];
                        _success = true;
                    }
                }
            }
            catch { }
        }

        private void SaveExchangeRatesToDatabase()
        {
            try
            {
                using (DB db = new DB())
                {
                    db.CreateCommand("SaveCurrentExchangeRates");

                    db.AddParameter("AUD", System.Data.SqlDbType.Float, _aud);
                    db.AddParameter("CAD", System.Data.SqlDbType.Float, _cad);
                    db.AddParameter("EUR", System.Data.SqlDbType.Float, _eur);
                    db.AddParameter("USD", System.Data.SqlDbType.Float, _usd);

                    db.ExecuteNonReader();

                    db.CommitTransaction();
                }
            }
            catch { }
        }

        private void LoadExchangeRatesFromFeed()
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load("http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
                _date = DateTime.Parse(xml.DocumentElement.ChildNodes[2].ChildNodes[0].Attributes["time"].Value);
                foreach (XmlNode item in xml.DocumentElement.ChildNodes[2].ChildNodes[0].ChildNodes)
                {
                    var rate = double.Parse(item.Attributes["rate"].Value);
                    switch (item.Attributes["currency"].Value)
                    {
                        case "GBP":
                            _gbpBase = 1.0 / rate;
                            break;
                        case "USD":
                            _usd = rate;
                            break;
                        case "AUD":
                            _aud = rate;
                            break;
                        case "CAD":
                            _cad = rate;
                            break;
                    }
                }
                _aud = _aud * _gbpBase * _costGbp;
                _cad = _cad * _gbpBase * _costGbp;
                _eur = 1.0 * _gbpBase * _costGbp;
                _usd = _usd * _gbpBase * _costGbp;
                _success = true;
            }
            catch { }
        }

        private void LoadExchangeRates()
        {
            _success = false;

            LoadExchangeRatesFromDatabase();

            if (! _success)
            {
                LoadExchangeRatesFromFeed();
                
                if (_success)
                {
                    SaveExchangeRatesToDatabase();
                }
            }

            if (! _success)
            {
                _usd = 0;
                _aud = 0;
                _eur = 0;
                _cad = 0;
            }
        }
    }
}