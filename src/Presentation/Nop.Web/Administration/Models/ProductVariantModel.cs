﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Validators;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models
{
    [Validator(typeof(ProductVariantValidator))]
    public class ProductVariantModel : BaseNopEntityModel, ILocalizedModel<ProductVariantLocalizedModel>
    {
        public ProductVariantModel()
        {
            Locales = new List<ProductVariantLocalizedModel>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        #region Standard properties

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ID")]
        public override int Id { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Sku")]
        [AllowHtml]
        public string Sku { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ManufacturerPartNumber")]
        [AllowHtml]
        public string ManufacturerPartNumber { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsGiftCard")]
        public bool IsGiftCard { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.GiftCardType")]
        public int GiftCardTypeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsDownload")]
        public bool IsDownload { get; set; }
        
        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Download")]
        [UIHint("Download")]
        public int DownloadId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.UnlimitedDownloads")]
        public bool UnlimitedDownloads { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MaxNumberOfDownloads")]
        public int MaxNumberOfDownloads { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DownloadExpirationDays")]
        public int? DownloadExpirationDays { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DownloadActivationType")]
        public int DownloadActivationTypeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.HasSampleDownload")]
        public bool HasSampleDownload { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SampleDownload")]
        [UIHint("Download")]
        public int SampleDownloadId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.HasUserAgreement")]
        public bool HasUserAgreement { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.UserAgreementText")]
        [AllowHtml]
        public string UserAgreementText { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsRecurring")]
        public bool IsRecurring { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringCycleLength")]
        public int RecurringCycleLength { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringCyclePeriod")]
        public int RecurringCyclePeriodId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringTotalCycles")]
        public int RecurringTotalCycles { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsShipEnabled")]
        public bool IsShipEnabled { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsFreeShipping")]
        public bool IsFreeShipping { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AdditionalShippingCharge")]
        public decimal AdditionalShippingCharge { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.TaxCategory")]
        public int TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ManageInventoryMethod")]
        public int ManageInventoryMethodId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayStockAvailability")]
        public bool DisplayStockAvailability { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayStockQuantity")]
        public bool DisplayStockQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MinStockQuantity")]
        public int MinStockQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.LowStockActivity")]
        public int LowStockActivityId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.NotifyAdminForQuantityBelow")]
        public int NotifyAdminForQuantityBelow { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BackorderMode")]
        public int BackorderModeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OrderMinimumQuantity")]
        public int OrderMinimumQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OrderMaximumQuantity")]
        public int OrderMaximumQuantity { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Warehouse")]
        public int WarehouseId { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }


        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisableBuyButton")]
        public bool DisableBuyButton { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.CallForPrice")]
        public bool CallForPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Price")]
        public decimal Price { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ProductCost")]
        public decimal ProductCost { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.CustomerEntersPrice")]
        public bool CustomerEntersPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MinimumCustomerEnteredPrice")]
        public decimal MinimumCustomerEnteredPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MaximumCustomerEnteredPrice")]
        public decimal MaximumCustomerEnteredPrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Weight")]
        public decimal Weight { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Length")]
        public decimal Length { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Width")]
        public decimal Width { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Height")]
        public decimal Height { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableStartDateTime")]
        [UIHint("DateNullable")]
        public DateTime? AvailableStartDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableEndDateTime")]
        [UIHint("DateNullable")]
        public DateTime? AvailableEndDateTimeUtc { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        #endregion

        #region Nested classes

        public class TierPriceModel : BaseNopEntityModel
        {
            public int ProductVariantId { get; set; }

            public int CustomerRoleId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole")]
            [UIHint("TierPriceCustomer")]
            public string CustomerRole { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.Quantity")]
            public int Quantity { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.Price")]
            //we don't name it Price because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's productVariant.Price1
            public decimal Price1 { get; set; }
        }

        public class ProductVariantAttributeModel : BaseNopEntityModel
        {
            public int ProductVariantId { get; set; }

            public int ProductAttributeId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.Attribute")]
            [UIHint("ProductAttribute")]
            public string ProductAttribute { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.TextPrompt")]
            [AllowHtml]
            public string TextPrompt { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.IsRequired")]
            public bool IsRequired { get; set; }

            public int AttributeControlTypeId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.AttributeControlType")]
            [UIHint("AttributeControlType")]
            public string AttributeControlType { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }

            public string ViewEditUrl { get; set; }
            public string ViewEditText { get; set; }
        }

        public class ProductVariantAttributeValueListModel : BaseNopModel
        {
            public int ProductVariantId { get; set; }

            public string ProductVariantName { get; set; }

            public int ProductVariantAttributeId { get; set; }

            public string ProductVariantAttributeName { get; set; }
        }

        [Validator(typeof(ProductVariantAttributeValueModelValidator))]
        public class ProductVariantAttributeValueModel : BaseNopEntityModel, ILocalizedModel<ProductVariantAttributeValueLocalizedModel>
        {
            public ProductVariantAttributeValueModel()
            {
                Locales = new List<ProductVariantAttributeValueLocalizedModel>();
            }

            public int ProductVariantAttributeId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
            public decimal PriceAdjustment { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
            public decimal WeightAdjustment { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected")]
            public bool IsPreSelected { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public IList<ProductVariantAttributeValueLocalizedModel> Locales { get; set; }
        }
        
        public class ProductVariantAttributeValueLocalizedModel : ILocalizedModelLocal
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }
        }

        #endregion

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ProductName")]
        public string ProductName { get; set; }

        //product attributes
        public int NumberOfAvailableProductAttributes { get; set; }

        public IList<ProductVariantLocalizedModel> Locales { get; set; }

        //discounts
        public List<Discount> AvailableDiscounts { get; set; }
        public int[] SelectedDiscountIds { get; set; }


        public bool HideNameAndDescriptionProperties { get; set; }
        public bool HidePublishedProperty { get; set; }
        public bool HideDisplayOrderProperty { get; set; }
    }
    public class ProductVariantLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }
}