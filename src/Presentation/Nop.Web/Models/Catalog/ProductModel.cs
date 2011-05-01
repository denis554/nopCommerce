﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public class ProductModel : BaseNopEntityModel
    {
        public ProductModel()
        {
            ProductPrice = new ProductPriceModel();
        }

        public string Name { get; set; }

        public string SeName { get; set; }

        public string ShortDescription { get; set; }

        public string ImageUrl { get; set; }

        public ProductPriceModel ProductPrice { get; set; }


		#region Nested Classes 
        
        public class ProductPriceModel
        {
            public string OldPrice { get; set; }

            public string Price {get;set;}
        }

		#endregion Nested Classes 
    }
}