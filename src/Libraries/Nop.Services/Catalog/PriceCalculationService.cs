
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core;
using Nop.Services.Discounts;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeParser _productAttributeParser;

        public PriceCalculationService(IWorkContext workContext,
            IDiscountService discountService,
            ICategoryService categoryService,
            IProductAttributeParser productAttributeParser)
        {
            this._workContext = workContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
        }
        
        #region Utilities

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected IList<Discount> GetAllowedDiscounts(ProductVariant productVariant, 
            Customer customer)
        {
            var allowedDiscounts = new List<Discount>();

            foreach (var discount in productVariant.AppliedDiscounts)
            {
                if (_discountService.IsDiscountValid(discount, customer) &&
                    discount.DiscountType == DiscountType.AssignedToSkus &&
                    !allowedDiscounts.Contains(discount))
                {
                    allowedDiscounts.Add(discount);
                }
            }

            var productCategories = _categoryService.GetProductCategoriesByProductId(productVariant.ProductId);
            foreach (var productCategory in productCategories)
            {
                var categoryDiscounts = productCategory.Category.AppliedDiscounts;
                foreach (var discount in categoryDiscounts)
                {
                    if (_discountService.IsDiscountValid(discount, customer) &&
                        discount.DiscountType == DiscountType.AssignedToCategories &&
                        !allowedDiscounts.Contains(discount))
                    {
                        allowedDiscounts.Add(discount);
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// Gets a preferred discount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">Customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <returns>Preferred discount</returns>
        protected Discount GetPreferredDiscount(ProductVariant productVariant,
            Customer customer, decimal additionalCharge = decimal.Zero, int quantity = 1)
        {
            var allowedDiscounts = GetAllowedDiscounts(productVariant, customer);
            decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
            var preferredDiscount = _discountService.GetPreferredDiscount(allowedDiscounts, finalPriceWithoutDiscount);
            return preferredDiscount;
        }

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">Customer</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Price</returns>
        protected decimal GetTierPrice(ProductVariant productVariant, Customer customer, int quantity)
        {
            var tierPrices = productVariant.TierPrices.OrderBy(tp => tp.Quantity).ToList();

            int previousQty = 1;
            decimal previousPrice = productVariant.Price;
            foreach (var tierPrice in tierPrices)
            {
                //check customer role requirement
                if (tierPrice.CustomerRole != null)
                {
                    if (customer == null)
                        continue;

                    var customerRoles = customer.CustomerRoles; //TODO filter active roles
                    if (customerRoles.Count == 0)
                        continue;

                    bool roleIsFound = false;
                    foreach (var customerRole in customerRoles)
                        if (customerRole == tierPrice.CustomerRole)
                            roleIsFound = true;

                    if (!roleIsFound)
                        continue;
                }


                //check quantity
                if (quantity < tierPrice.Quantity)
                    continue;
                if (tierPrice.Quantity < previousQty)
                    continue;

                //save new price
                previousPrice = tierPrice.Price;
                previousQty = tierPrice.Quantity;
            }
            
            return previousPrice;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public decimal GetFinalPrice(ProductVariant productVariant, 
            bool includeDiscounts)
        {
            var customer = _workContext.CurrentCustomer;
            return GetFinalPrice(productVariant, customer, includeDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public decimal GetFinalPrice(ProductVariant productVariant, 
            Customer customer, 
            bool includeDiscounts)
        {
            return GetFinalPrice(productVariant, customer, decimal.Zero, includeDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        public decimal GetFinalPrice(ProductVariant productVariant, 
            Customer customer, 
            decimal additionalCharge, 
            bool includeDiscounts)
        {
            return GetFinalPrice(productVariant, customer, additionalCharge, 
                includeDiscounts, 1);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <returns>Final price</returns>
        public decimal GetFinalPrice(ProductVariant productVariant, 
            Customer customer,
            decimal additionalCharge, 
            bool includeDiscounts, 
            int quantity)
        {
            decimal result = decimal.Zero;

            //initial price
            decimal initialPrice = productVariant.Price;

            //tier prices
            if (productVariant.TierPrices.Count > 0)
            {
                decimal tierPrice = GetTierPrice(productVariant, customer, quantity);
                initialPrice = Math.Min(initialPrice, tierPrice);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                Discount appliedDiscount = null;
                decimal discountAmount = GetDiscountAmount(productVariant, customer, additionalCharge, quantity, out appliedDiscount);
                result = initialPrice + additionalCharge - discountAmount;
            }
            else
            {
                result = initialPrice + additionalCharge;
            }
            if (result < decimal.Zero)
                result = decimal.Zero;
            return result;
        }



        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ProductVariant productVariant)
        {
            var customer = _workContext.CurrentCustomer;
            return GetDiscountAmount(productVariant, customer, decimal.Zero);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer)
        {
            return GetDiscountAmount(productVariant, customer, decimal.Zero);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer, 
            decimal additionalCharge)
        {
            Discount appliedDiscount = null;
            return GetDiscountAmount(productVariant, customer, additionalCharge, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer,
            decimal additionalCharge, 
            out Discount appliedDiscount)
        {
            return GetDiscountAmount(productVariant, customer, additionalCharge, 1, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ProductVariant productVariant, 
            Customer customer,
            decimal additionalCharge,
            int quantity, 
            out Discount appliedDiscount)
        {
            decimal appliedDiscountAmount = decimal.Zero;

            //we don't apply discounts to products with price entered by a customer
            if (productVariant.CustomerEntersPrice)
            {
                appliedDiscount = null;
                return appliedDiscountAmount;
            }

            appliedDiscount = GetPreferredDiscount(productVariant, customer, additionalCharge, quantity);
            if (appliedDiscount != null)
            {
                decimal finalPriceWithoutDiscount = GetFinalPrice(productVariant, customer, additionalCharge, false, quantity);
                appliedDiscountAmount = appliedDiscount.GetDiscountAmount(finalPriceWithoutDiscount);
            }

            return appliedDiscountAmount;
        }

        
        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        public decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
            return GetUnitPrice(shoppingCartItem, includeDiscounts) * shoppingCartItem.Quantity;
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
            var customer = shoppingCartItem.Customer;
            decimal finalPrice = decimal.Zero;
            var productVariant = shoppingCartItem.ProductVariant;
            if (productVariant != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.AttributesXml);
                foreach (var pvaValue in pvaValues)
                {
                    attributesTotalPrice += pvaValue.PriceAdjustment;
                }

                if (productVariant.CustomerEntersPrice)
                {
                    finalPrice = shoppingCartItem.CustomerEnteredPrice;
                }
                else
                {
                    finalPrice = GetFinalPrice(productVariant,
                        customer,
                        attributesTotalPrice,
                        includeDiscounts,
                        shoppingCartItem.Quantity);
                }
            }

            //finalPrice = Math.Round(finalPrice, 2);

            return finalPrice;
        }
        


        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem)
        {
            Discount appliedDiscount;
            return GetDiscountAmount(shoppingCartItem, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem, out Discount appliedDiscount)
        {
            var customer = shoppingCartItem.Customer;
            appliedDiscount = null;
            decimal discountAmount = decimal.Zero;
            var productVariant = shoppingCartItem.ProductVariant;
            if (productVariant != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.AttributesXml);
                foreach (var pvaValue in pvaValues)
                {
                    attributesTotalPrice += pvaValue.PriceAdjustment;
                }

                decimal productVariantDiscountAmount = GetDiscountAmount(productVariant, customer, attributesTotalPrice, shoppingCartItem.Quantity, out appliedDiscount);
                discountAmount = productVariantDiscountAmount * shoppingCartItem.Quantity;
            }

            //discountAmount = Math.Round(discountAmount, 2);
            return discountAmount;
        }
        
        #endregion
    }
}
