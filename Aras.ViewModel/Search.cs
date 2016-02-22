﻿/*  
  Aras.ViewModel provides a .NET library for building Aras Innovator Applications

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.ViewModel
{
    public abstract class Search<T> : Control where T : Model.Item
    {
        private List<String> _propertyNames;
        
        public IEnumerable<String> PropertyNames
        {
            get
            {
                return this._propertyNames;
            }
        }

        public void SetPropertyNames(String Names)
        {
            this._propertyNames.Clear();
            this.AddToPropertyNames(Names);
        }

        public void AddToPropertyNames(String Names)
        {
            String[] parts = Names.Split(',');

            foreach (String name in parts)
            {
                if (!this._propertyNames.Contains(name))
                {
                    this._propertyNames.Add(name);
                }
            }
        }

        private Int32 _page;
        [ViewModel.Attributes.Property("Page", Aras.ViewModel.Attributes.PropertyTypes.Int32, false)]
        public Int32 Page
        {
            get
            {
                return this._page;
            }
            set
            {
                if (this._page != value)
                {
                    this._page = value;

                    if (this.Query != null)
                    {
                        this.Query.Page = value;
                        this.RefreshControl();
                        this.OnPropertyChanged("Page");
                    }
                }
            }
        }

        private Int32 _pageSize;
        [ViewModel.Attributes.Property("PageSize", Aras.ViewModel.Attributes.PropertyTypes.Int32, false)]
        public Int32 PageSize
        {
            get
            {
                return this._pageSize;
            }
            set
            {
                if (this._pageSize != value)
                {
                    this._pageSize = value;

                    if (this.Query != null)
                    {
                        this.Query.PageSize = this._pageSize;
                        this.RefreshControl();
                        this.OnPropertyChanged("PageSize");
                    }
                }
            }
        }

        private Int32 _noPages;
        [ViewModel.Attributes.Property("NoPages", Aras.ViewModel.Attributes.PropertyTypes.Int32, true)]
        public Int32 NoPages
        {
            get
            {
                return this._noPages;
            }
            set
            {
                if (this._noPages != value)
                {
                    this._noPages = value;
                    this.OnPropertyChanged("NoPages");
                }
            }
        }

        [ViewModel.Attributes.Property("Grid", Aras.ViewModel.Attributes.PropertyTypes.Control, true)]
        public Grid Grid { get; private set; }

        protected override void AfterBindingChanged()
        {
            base.AfterBindingChanged();

            // Load Columns
            this.LoadColumns();

            // Load Rows
            this.LoadRows();
        }

        protected override void RefreshControl()
        {
            base.RefreshControl();

            // Refresh Query
            if (this.Query != null)
            {
                this.Query.Refresh();
            }

            // Load Grid
            this.LoadRows();
        }

        private List<Model.PropertyType> PropertyTypes;

        private void LoadColumns()
        {
            if ((this.Binding != null) && (this.Binding is Model.Store<T>))
            {
                this.Grid.Columns.Clear();

                // Build List of PropertyTypes
                this.PropertyTypes = new List<Model.PropertyType>();

                foreach(String propertyname in this.PropertyNames)
                {
                    if (((Model.Store<T>)this.Binding).ItemType.HasPropertyType(propertyname))
                    {
                        this.PropertyTypes.Add(((Model.Store<T>)this.Binding).ItemType.PropertyType(propertyname));
                    }
                }

                foreach(Model.PropertyType proptype in this.PropertyTypes)
                {
                    this.Grid.AddColumn(proptype.Name, proptype.Label);
                }
            }
            else
            {
                // Clear Columns
                this.Grid.Columns.Clear();
            }
        }

        private Model.Query<T> _query;
        private Model.Query<T> Query
        {
            get
            {
                if (this._query == null)
                {
                    if (this.Binding != null && this.Binding is Model.Store<T>)
                    {
                        // Create Query
                        this._query = ((Model.Store<T>)this.Binding).Query();
                        
                        // Switch on Paging
                        this._query.Paging = true;

                        // Update Page Size on Control
                        this.PageSize = this._query.PageSize;

                        // Update Page on Control
                        this.Page = this._query.Page;
                    }
                }

                return this._query;
            }
        }

        private void LoadRows()
        {
            if (this.Query != null)
            {
                this.Grid.NoRows = this.Query.Count();

                for(int i=0; i<this.Grid.NoRows; i++)
                {
                    T item = this.Query[i];

                    for(int j=0; j<this.PropertyTypes.Count(); j++)
                    {
                        Model.PropertyType proptype = this.PropertyTypes[j];
                        Model.Property property = item.Property(proptype);

                        if (this.Grid.Rows[i].Cells[j].Value == null)
                        {
                            switch(property.GetType().Name)
                            {
                                case "String":
                                    this.Grid.Rows[i].Cells[j].Value = new Properties.String();
                                    break;
                                default:
                                    throw new Model.Exceptions.ArgumentException("PropertyType not implmented: " + property.GetType().Name);
                            }
                        }

                        this.Grid.Rows[i].Cells[j].Value.Binding = property;
                    }
                }
            }
            else
            {
                // Clear all Rows
                this.Grid.NoRows = 0;
            }
        }

        public Search()
            :base()
        {
            this.Grid = new Grid();
            this._propertyNames = new List<String>();
        }
    }
}