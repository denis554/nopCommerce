﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.Tax.Free
{
    /// <summary>
    /// Free tax provider
    /// </summary>
    public class FreeTaxProvider : BasePlugin, ITaxProvider
    {
        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public override string FriendlyName
        {
            get { return "Free tax rate provider"; }
        }

        /// <summary>
        /// Gets or sets the system name
        /// </summary>
        public override string SystemName
        {
            get { return "Tax.FreeTaxRate"; }
        }

        /// <summary>
        /// Gets the author
        /// </summary>
        public override string Author
        {
            get { return "nopCommerce team"; }
        }

        /// <summary>
        /// Gets the version
        /// </summary>
        public override string Version
        {
            get { return "1.00"; }
        }
        
        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="calculateTaxRequest">Tax calculation request</param>
        /// <returns>Tax</returns>
        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult()
            {
                 TaxRate = decimal.Zero
            };
            return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = null;
            controllerName = null;
            routeValues = null;
        }
    }
}
