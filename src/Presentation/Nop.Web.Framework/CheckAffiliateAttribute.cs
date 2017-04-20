﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Services.Affiliates;
using Nop.Services.Customers;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Represents filter attribute that checks and updates affiliate of customer
    /// </summary>
    public class CheckAffiliateAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public CheckAffiliateAttribute() : base(typeof(CheckAffiliateFilter))
        {
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter that checks and updates affiliate of customer
        /// </summary>
        private class CheckAffiliateFilter : IActionFilter
        {
            #region Constants

            private const string AFFILIATE_ID_QUERY_PARAMETER_NAME = "affiliateid";
            private const string AFFILIATE_FRIENDLYURLNAME_QUERY_PARAMETER_NAME = "affiliate";

            #endregion

            #region Fields

            private readonly IAffiliateService _affiliateService;
            private readonly ICustomerService _customerService;
            private readonly IWorkContext _workContext;

            #endregion

            #region Ctor

            public CheckAffiliateFilter(IAffiliateService affiliateService, 
                ICustomerService customerService,
                IWorkContext workContext)
            {
                this._affiliateService = affiliateService;
                this._customerService = customerService;
                this._workContext = workContext;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Set the affiliate identifier of current customer
            /// </summary>
            /// <param name="affiliate">Affiliate</param>
            protected void SetCustomerAffiliateId(Affiliate affiliate)
            {
                if (affiliate == null || affiliate.Deleted || !affiliate.Active)
                    return;

                if (affiliate.Id == _workContext.CurrentCustomer.AffiliateId)
                    return;

                //update affiliate identifier
                _workContext.CurrentCustomer.AffiliateId = affiliate.Id;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                    return;

                //check request query parameters
                var request = context.HttpContext.Request;
                if (request.Query == null || !request.Query.Any())
                    return;

                //try to find by ID
                var affiliateIds = request.Query[AFFILIATE_ID_QUERY_PARAMETER_NAME];
                if (affiliateIds.Any() && int.TryParse(affiliateIds.FirstOrDefault(), out int affiliateId) && affiliateId > 0)
                {
                    SetCustomerAffiliateId(_affiliateService.GetAffiliateById(affiliateId));
                    return;
                }

                //try to find by friendly name
                var affiliateNames = request.Query[AFFILIATE_FRIENDLYURLNAME_QUERY_PARAMETER_NAME];
                if (affiliateNames.Any())
                {
                    var affiliateName = affiliateNames.FirstOrDefault();
                    if (!string.IsNullOrEmpty(affiliateName))
                        SetCustomerAffiliateId(_affiliateService.GetAffiliateByFriendlyUrlName(affiliateName));
                }
            }

            /// <summary>
            /// Called after the action executes, before the action result
            /// </summary>
            /// <param name="context">A context for action filters</param>
            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            #endregion
        }

        #endregion
    }
}