using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;

namespace Nop.Plugin.DiscountRules.HasOneProduct
{
    public partial class HasOneProductDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public override string FriendlyName
        {
            get { return "Customer has one of these product variants in the cart"; }
        }

        /// <summary>
        /// Gets or sets the system name
        /// </summary>
        public override string SystemName
        {
            get { return "DiscountRequirement.HasOneProduct"; }
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
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.DiscountRequirement == null)
                throw new NopException("Discount requirement is not set");

            if (String.IsNullOrWhiteSpace(request.DiscountRequirement.RestrictedProductVariantIds))
                return true;

            if (request.Customer == null)
                return false;

            var restrictedProductVariantIds = new List<int>();
            try
            {
                restrictedProductVariantIds = request.DiscountRequirement.RestrictedProductVariantIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();
            }
            catch
            {
                //error parsing
                return false;
            }
            if (restrictedProductVariantIds.Count == 0)
                return false;

            //cart
            var cart = request.Customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);
            

            bool found = false;
            foreach (var restrictedPvId in restrictedProductVariantIds)
            {
                foreach (var sci in cart)
                {
                    if (restrictedPvId == sci.ProductVariantId)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (found)
                return true;

            return false;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesHasOneProduct/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }
    }
}