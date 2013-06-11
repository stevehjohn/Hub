using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TabDocuments
{
    internal class Pager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Pager()
        {
            PageSize = Properties.Settings.Default.CollectionViewPageSize;
            Page = 1;
        }

        private int _page;

        private int _pageSize;

        private int _totalItems;

        public int FirstItemIndex { get { return (Page - 1) * PageSize; } }

        public int Page 
        { 
            get
            {
                return _page;
            }
            set
            {
                if (value > Pages)
                {
                    _page = Pages;
                }
                else if (value < 1)
                {
                    _page = 1;
                }
                else
                {
                    _page = value;
                }
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Page"));
            }
        }

        public int Pages 
        { 
            get 
            { 
                return (int) Math.Ceiling((double) TotalItems / PageSize); 
            } 
        }

        public int PageSize 
        { 
            get
            {
                return _pageSize;
            }
            set
            {
                if (value < 1)
                {
                    _pageSize = 1;
                }
                else
                {
                    _pageSize = value;
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PageSize"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Pages"));
                }
                Properties.Settings.Default.CollectionViewPageSize = _pageSize;
                Properties.Settings.Default.Save();
            }
        }

        public int TotalItems 
        {
            get { return _totalItems; }
            set
            {
                _totalItems = value;
                if (Page == 0)
                {
                    Page = 1;
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TotalItems"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Pages"));
                }
            }
        }
    }
}
