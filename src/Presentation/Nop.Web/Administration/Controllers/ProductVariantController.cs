﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ProductVariantController : BaseNopController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICopyProductService _copyProductService;
        private readonly IProductTagService _productTagService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICurrencyService _currencyService;
        private readonly IDownloadService _downloadService;

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly AdminAreaSettings _adminAreaSettings;
        #endregion

        #region Constructors

        public ProductVariantController(IProductService productService,
            ICopyProductService copyProductService,
            IProductTagService productTagService, IPictureService pictureService,
            ILanguageService languageService, ILocalizedEntityService localizedEntityService,
            IDiscountService discountService, ICustomerService customerService,
            ILocalizationService localizationService, IProductAttributeService productAttributeService,
            ITaxCategoryService taxCategoryService, IWorkContext workContext,
            IProductAttributeFormatter productAttributeFormatter, IShoppingCartService shoppingCartService,
            IProductAttributeParser productAttributeParser, ICustomerActivityService customerActivityService,
            IPermissionService permissionService, 
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICurrencyService currencyService, IDownloadService downloadService, 
            CatalogSettings catalogSettings, CurrencySettings currencySettings,
            IMeasureService measureService, MeasureSettings measureSettings,
            AdminAreaSettings adminAreaSettings)
        {
            this._localizedEntityService = localizedEntityService;
            this._pictureService = pictureService;
            this._productTagService = productTagService;
            this._languageService = languageService;
            this._productService = productService;
            this._copyProductService = copyProductService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._taxCategoryService = taxCategoryService;
            this._workContext = workContext;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeParser = productAttributeParser;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._currencyService = currencyService;
            this._downloadService = downloadService;

            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._adminAreaSettings = adminAreaSettings;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        protected void UpdateLocales(ProductVariant variant, ProductVariantModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Description,
                                                               localized.Description,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdatePictureSeoNames(ProductVariant variant)
        {
            var picture = _pictureService.GetPictureById(variant.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(variant.FullProductName));
        }

        [NonAction]
        protected void UpdateAttributeValueLocales(ProductVariantAttributeValue pvav, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pvav,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected void PrepareProductModel(ProductVariantModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.ProductName = product.Name;
        }

        [NonAction]
        protected void PrepareProductVariantModel(ProductVariantModel model, ProductVariant variant, bool setPredefinedValues)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = variant != null && !setPredefinedValues && tc.Id == variant.TaxCategoryId });

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;

            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.DisplayOrder = 1;

                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.Published = true;
            }
        }

        [NonAction]
        protected void PrepareDiscountModel(ProductVariantModel model, ProductVariant variant, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
            model.AvailableDiscounts = discounts.ToList();

            if (!excludeProperties)
            {
                model.SelectedDiscountIds = variant.AppliedDiscounts.Select(d => d.Id).ToArray();
            }
        }

        [NonAction]
        protected void PrepareProductAttributesMapping(ProductVariantModel model, ProductVariant variant)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableProductAttributes = _productAttributeService.GetAllProductAttributes().Count;
        }

        [NonAction]
        protected void PrepareAddProductAttributeCombinationModel(AddProductVariantAttributeCombinationModel model, ProductVariant variant)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (variant == null)
                throw new ArgumentNullException("variant");

            model.ProductVariantId = variant.Id;
            model.StockQuantity = 10000;

            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(variant.Id);
            foreach (var attribute in productVariantAttributes)
            {
                var pvaModel = new AddProductVariantAttributeCombinationModel.ProductVariantAttributeModel()
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);
                    foreach (var pvaValue in pvaValues)
                    {
                        var pvaValueModel = new AddProductVariantAttributeCombinationModel.ProductVariantAttributeValueModel()
                        {
                            Id = pvaValue.Id,
                            Name = pvaValue.Name,
                            IsPreSelected = pvaValue.IsPreSelected
                        };
                        pvaModel.Values.Add(pvaValueModel);
                    }
                }

                model.ProductVariantAttributes.Add(pvaModel);
            }
        }
        
        [NonAction]
        private void PrepareCopyProductVariantModel(ProductVariantModel model, ProductVariant productVariant)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productVariant != null)
            {
                model.CopyProductVariantModel.Id = productVariant.Id;
                if (!String.IsNullOrEmpty(productVariant.Name))
                    model.CopyProductVariantModel.Name = "Copy of " + productVariant.Name;
                model.CopyProductVariantModel.Published = true;
            }
        }
        #endregion

        #region List / Create / Edit / Delete

        public ActionResult Create(int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product review found with the specified id
                return RedirectToAction("Edit", "Product", new { id = productId });

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductVariantModel()
            {
                ProductId = productId,
            };
            //locales
            AddLocales(_languageService, model.Locales);
            //common
            PrepareProductModel(model, product);
            PrepareProductVariantModel(model, null, true);
            //attributes
            PrepareProductAttributesMapping(model, null);
            //discounts
            PrepareDiscountModel(model, null, true);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(ProductVariantModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //If we got this far, something failed, redisplay form
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                var variant = model.ToEntity();
                variant.CreatedOnUtc = DateTime.UtcNow;
                variant.UpdatedOnUtc = DateTime.UtcNow;
                //insert variant
                _productService.InsertProductVariant(variant);
                //locales
                UpdateLocales(variant, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        variant.AppliedDiscounts.Add(discount);
                }
                _productService.UpdateProductVariant(variant);
                //update "HasDiscountsApplied" property
                _productService.UpdateHasDiscountsApplied(variant);
                //update picture seo file name
                UpdatePictureSeoNames(variant);

                //activity log
                _customerActivityService.InsertActivity("AddNewProductVariant", _localizationService.GetResource("ActivityLog.AddNewProductVariant"), variant.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = variant.Id }) : RedirectToAction("Edit", "Product", new { id = variant.ProductId });
            }

            //common
            PrepareProductModel(model, product);
            PrepareProductVariantModel(model, null, false);
            //attributes
            PrepareProductAttributesMapping(model, null);
            //discounts
            PrepareDiscountModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(id);
            if (variant == null || variant.Deleted)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var product = variant.Product;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = variant.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = variant.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = variant.GetLocalized(x => x.Description, languageId, false, false);
            });
            //common
            PrepareProductModel(model, variant.Product);
            PrepareProductVariantModel(model, variant, false);
            //attributes
            PrepareProductAttributesMapping(model, variant);
            //discounts
            PrepareDiscountModel(model, variant, false);
            //copy variant model
            PrepareCopyProductVariantModel(model, variant);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(ProductVariantModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(model.Id);
            if (variant == null || variant.Deleted)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var product = variant.Product;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                int prevPictureId = variant.PictureId;
                var prevStockQuantity = variant.StockQuantity;
                variant = model.ToEntity(variant);
                variant.UpdatedOnUtc = DateTime.UtcNow;
                //save variant
                _productService.UpdateProductVariant(variant);
                //locales
                UpdateLocales(variant, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new role
                        if (variant.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                            variant.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //removed role
                        if (variant.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                            variant.AppliedDiscounts.Remove(discount);
                    }
                }
                _productService.UpdateProductVariant(variant);
                //update "HasDiscountsApplied" property
                _productService.UpdateHasDiscountsApplied(variant);
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != variant.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(variant);
                //back in stock notifications
                if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    variant.BackorderMode == BackorderMode.NoBackorders &&
                    variant.AllowBackInStockSubscriptions &&
                    variant.StockQuantity > 0 &&
                    prevStockQuantity <= 0 &&
                    variant.Published &&
                    !variant.Deleted)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(variant);
                }
                //activity log
                _customerActivityService.InsertActivity("EditProductVariant", _localizationService.GetResource("ActivityLog.EditProductVariant"), variant.FullProductName);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Updated"));
                return continueEditing ? RedirectToAction("Edit", model.Id) : RedirectToAction("Edit", "Product", new { id = variant.ProductId });
            }

            //If we got this far, something failed, redisplay form
            //common
            PrepareProductModel(model, variant.Product);
            PrepareProductVariantModel(model, variant, false);
            //attributes
            PrepareProductAttributesMapping(model, variant);
            //discounts
            PrepareDiscountModel(model, variant, true);
            //copy variant model
            PrepareCopyProductVariantModel(model, variant);
            return View(model);
        }
        
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(id);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var productId = variant.ProductId;
            var product = variant.Product;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            _productService.DeleteProductVariant(variant);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductVariant", _localizationService.GetResource("ActivityLog.DeleteProductVariant"), variant.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Deleted"));
            return RedirectToAction("Edit", "Product", new { id = productId });
        }

        [HttpPost]
        public ActionResult CopyProductVariant(ProductVariantModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductVariantModel;
            try
            {
                var originalProductVariant = _productService.GetProductVariantById(copyModel.Id);

                var product = originalProductVariant.Product;

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List", "Product");

                var newProductVariant = _copyProductService.CopyProductVariant(originalProductVariant,
                    originalProductVariant.Product.Id, copyModel.Name, copyModel.Published, true);
                SuccessNotification("The product variant has been copied successfully");
                return RedirectToAction("Edit", new { id = newProductVariant.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Low stock reports

        public ActionResult LowStockReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View();
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult LowStockReportList(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var allVariants = _productService.GetLowStockProductVariants(vendorId);
            var model = new GridModel<ProductVariantModel>()
            {
                Data = allVariants.PagedForCommand(command).Select(x =>
                {
                    var variantModel = x.ToModel();
                    //Full product variant name
                    variantModel.Name = x.FullProductName;
                    return variantModel;
                }),
                Total = allVariants.Count
            };
            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Bulk editing

        public ActionResult BulkEdit()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new BulkEditListModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(showHidden: true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            return View(model);
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult BulkEditSelect(GridCommand command, BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            int vendorId = 0;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var productVariants = _productService.SearchProductVariants(model.SearchCategoryId,
                model.SearchManufacturerId, vendorId, model.SearchProductName, false,
                command.Page - 1, command.PageSize, true);
            var gridModel = new GridModel();
            gridModel.Data = productVariants.Select(x =>
            {
                var productVariantModel = new BulkEditProductVariantModel()
                {
                    Id = x.Id,
                    Name =  x.FullProductName,
                    Sku = x.Sku,
                    OldPrice = x.OldPrice,
                    Price = x.Price,
                    ManageInventoryMethod = x.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = x.StockQuantity,
                    Published = x.Published
                };

                return productVariantModel;
            });
            gridModel.Total = productVariants.TotalCount;
            return new JsonResult
            {
                Data = gridModel
            };
        }
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult BulkEditSave(GridCommand command, 
            [Bind(Prefix = "updated")]IEnumerable<BulkEditProductVariantModel> updatedProductVariants,
            [Bind(Prefix = "deleted")]IEnumerable<BulkEditProductVariantModel> deletedProductVariants,
            BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (updatedProductVariants != null)
            {
                foreach (var pvModel in updatedProductVariants)
                {
                    //update
                    var pv = _productService.GetProductVariantById(pvModel.Id);
                    if (pv != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && pv.Product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        pv.Sku = pvModel.Sku;
                        pv.Price = pvModel.Price;
                        pv.OldPrice = pvModel.OldPrice;
                        pv.StockQuantity = pvModel.StockQuantity;
                        pv.Published = pvModel.Published;
                        _productService.UpdateProductVariant(pv);
                    }
                }
            }
            if (deletedProductVariants != null)
            {
                foreach (var pvModel in deletedProductVariants)
                {
                    //delete
                    var pv = _productService.GetProductVariantById(pvModel.Id);
                    if (pv != null)
                    {
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && pv.Product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        _productService.DeleteProductVariant(pv);
                    }
                }
            }
            return BulkEditSelect(command, model);
        }
        #endregion

        #region Tier prices

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var tierPrices = productVariant.TierPrices;
            var tierPricesModel = tierPrices
                .Select(x =>
                {
                    return new ProductVariantModel.TierPriceModel()
                    {
                        Id = x.Id,
                        CustomerRole = x.CustomerRoleId.HasValue ? _customerService.GetCustomerRoleById(x.CustomerRoleId.Value).Name : _localizationService.GetResource("Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole.AllRoles"),
                        ProductVariantId = x.ProductVariantId,
                        CustomerRoleId = x.CustomerRoleId.HasValue ? x.CustomerRoleId.Value : 0,
                        Quantity = x.Quantity,
                        Price1 = x.Price
                    };
                })
                .ToList();

            var model = new GridModel<ProductVariantModel.TierPriceModel>
            {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceInsert(GridCommand command, ProductVariantModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var variant = _productService.GetProductVariantById(model.ProductVariantId);
                if (variant != null)
                {
                    var product = _productService.GetProductById(variant.ProductId);
                    if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    {
                        return Content("This is not your product");
                    }
                }
            }

            var tierPrice = new TierPrice()
            {
                ProductVariantId = model.ProductVariantId,
                CustomerRoleId = Int32.Parse(model.CustomerRole) != 0 ? Int32.Parse(model.CustomerRole) : (int?)null, //use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it
                Quantity = model.Quantity,
                Price = model.Price1
            };
            _productService.InsertTierPrice(tierPrice);

            //update "HasTierPrices" property
            var productVariant = _productService.GetProductVariantById(model.ProductVariantId);
            _productService.UpdateHasTierPricesProperty(productVariant);

            return TierPriceList(command, model.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceUpdate(GridCommand command, ProductVariantModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(model.Id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var variant = _productService.GetProductVariantById(tierPrice.ProductVariantId);
                if (variant != null)
                {
                    var product = _productService.GetProductById(variant.ProductId);
                    if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    {
                        return Content("This is not your product");
                    }
                }
            }

            //use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it
            tierPrice.CustomerRoleId = Int32.Parse(model.CustomerRole) != 0 ? Int32.Parse(model.CustomerRole) : (int?)null;
            tierPrice.Quantity = model.Quantity;
            tierPrice.Price = model.Price1;
            _productService.UpdateTierPrice(tierPrice);

            return TierPriceList(command, tierPrice.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

            var productVariantId = tierPrice.ProductVariantId;
            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _productService.DeleteTierPrice(tierPrice);

            //update "HasTierPrices" property
            _productService.UpdateHasTierPricesProperty(productVariant);

            return TierPriceList(command, productVariantId);
        }

        #endregion

        #region Product variant attributes

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariantId);
            var productVariantAttributesModel = productVariantAttributes
                .Select(x =>
                {
                    var pvaModel =  new ProductVariantModel.ProductVariantAttributeModel()
                    {
                        Id = x.Id,
                        ProductVariantId = x.ProductVariantId,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId).Name,
                        ProductAttributeId = x.ProductAttributeId,
                        TextPrompt = x.TextPrompt,
                        IsRequired = x.IsRequired,
                        AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeControlTypeId = x.AttributeControlTypeId,
                        DisplayOrder1 = x.DisplayOrder
                    };

                    if (x.ShouldHaveValues())
                    {
                        pvaModel.ViewEditUrl = Url.Action("EditAttributeValues", "ProductVariant", new { productVariantAttributeId = x.Id });
                        pvaModel.ViewEditText = string.Format(_localizationService.GetResource("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.ViewLink"), x.ProductVariantAttributeValues != null ? x.ProductVariantAttributeValues.Count : 0);
                    }
                    return pvaModel;
                })
                .ToList();

            var model = new GridModel<ProductVariantModel.ProductVariantAttributeModel>
            {
                Data = productVariantAttributesModel,
                Total = productVariantAttributesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeInsert(GridCommand command, ProductVariantModel.ProductVariantAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var variant = _productService.GetProductVariantById(model.ProductVariantId);
                if (variant != null)
                {
                    var product = _productService.GetProductById(variant.ProductId);
                    if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    {
                        return Content("This is not your product");
                    }
                }
            }

            var pva = new ProductVariantAttribute()
            {
                ProductVariantId = model.ProductVariantId,
                ProductAttributeId = Int32.Parse(model.ProductAttribute), //use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it
                TextPrompt = model.TextPrompt,
                IsRequired = model.IsRequired,
                AttributeControlTypeId = Int32.Parse(model.AttributeControlType), //use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it
                DisplayOrder = model.DisplayOrder1
            };
            _productAttributeService.InsertProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, model.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttrbiuteUpdate(GridCommand command, ProductVariantModel.ProductVariantAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(model.Id);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var variant = _productService.GetProductVariantById(pva.ProductVariantId);
                if (variant != null)
                {
                    var product = _productService.GetProductById(variant.ProductId);
                    if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                    {
                        return Content("This is not your product");
                    }
                }
            }

            //use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it
            pva.ProductAttributeId = Int32.Parse(model.ProductAttribute);
            pva.TextPrompt = model.TextPrompt;
            pva.IsRequired = model.IsRequired;
            //use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it
            pva.AttributeControlTypeId = Int32.Parse(model.AttributeControlType);
            pva.DisplayOrder = model.DisplayOrder1;
            _productAttributeService.UpdateProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, pva.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(id);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            var productVariantId = pva.ProductVariantId;
            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _productAttributeService.DeleteProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, productVariantId);
        }

        #endregion

        #region Product variant attribute values

        //list
        public ActionResult EditAttributeValues(int productVariantAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pva.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            var model = new ProductVariantModel.ProductVariantAttributeValueListModel()
            {
                ProductVariantName = pva.ProductVariant.Product.Name + " " + pva.ProductVariant.Name,
                ProductVariantId = pva.ProductVariantId,
                ProductVariantAttributeName = pva.ProductAttribute.Name,
                ProductVariantAttributeId = pva.Id,
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAttributeValueList(int productVariantAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pva.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var values = _productAttributeService.GetProductVariantAttributeValues(productVariantAttributeId);
            var gridModel = new GridModel<ProductVariantModel.ProductVariantAttributeValueModel>
            {
                Data = values.Select(x =>
                {
                    return new ProductVariantModel.ProductVariantAttributeValueModel()
                    {
                        Id = x.Id,
                        ProductVariantAttributeId = x.ProductVariantAttributeId,
                        Name = x.ProductVariantAttribute.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                        ColorSquaresRgb = x.ColorSquaresRgb,
                        PriceAdjustment = x.PriceAdjustment,
                        WeightAdjustment = x.WeightAdjustment,
                        IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder,
                    };
                }),
                Total = values.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        //create
        public ActionResult ProductAttributeValueCreatePopup(int productAttributeAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(productAttributeAttributeId);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pva.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            var model = new ProductVariantModel.ProductVariantAttributeValueModel();
            model.ProductVariantAttributeId = productAttributeAttributeId;

            //color squares
            model.DisplayColorSquaresRgb = pva.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueCreatePopup(string btnId, string formId, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(model.ProductVariantAttributeId);
            if (pva == null)
                //No product variant attribute found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pva.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            if (pva.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    var color = System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            if (ModelState.IsValid)
            {
                var pvav = new ProductVariantAttributeValue()
                {
                    ProductVariantAttributeId = model.ProductVariantAttributeId,
                    Name = model.Name,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };

                _productAttributeService.InsertProductVariantAttributeValue(pvav);
                UpdateAttributeValueLocales(pvav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult ProductAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(id);
            if (pvav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pvav.ProductVariantAttribute.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            var model = new ProductVariantModel.ProductVariantAttributeValueModel()
            {
                ProductVariantAttributeId = pvav.ProductVariantAttributeId,
                Name = pvav.Name,
                ColorSquaresRgb = pvav.ColorSquaresRgb,
                DisplayColorSquaresRgb = pvav.ProductVariantAttribute.AttributeControlType == AttributeControlType.ColorSquares,
                PriceAdjustment = pvav.PriceAdjustment,
                WeightAdjustment = pvav.WeightAdjustment,
                IsPreSelected = pvav.IsPreSelected,
                DisplayOrder = pvav.DisplayOrder
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pvav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueEditPopup(string btnId, string formId, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(model.Id);
            if (pvav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pvav.ProductVariantAttribute.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            if (pvav.ProductVariantAttribute.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    var color = System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            if (ModelState.IsValid)
            {
                pvav.Name = model.Name;
                pvav.ColorSquaresRgb = model.ColorSquaresRgb;
                pvav.PriceAdjustment = model.PriceAdjustment;
                pvav.WeightAdjustment = model.WeightAdjustment;
                pvav.IsPreSelected = model.IsPreSelected;
                pvav.DisplayOrder = model.DisplayOrder;
                _productAttributeService.UpdateProductVariantAttributeValue(pvav);

                UpdateAttributeValueLocales(pvav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        
        //delete
        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAttributeValueDelete(int pvavId, int productVariantAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(pvavId);
            if (pvav == null)
                throw new ArgumentException("No product variant attribute value found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = pvav.ProductVariantAttribute.ProductVariant;
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            _productAttributeService.DeleteProductVariantAttributeValue(pvav);

            return ProductAttributeValueList(productVariantAttributeId, command);
        }

        #endregion

        #region Product variant attribute combinations

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeCombinationList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productVariantAttributeCombinations = _productAttributeService.GetAllProductVariantAttributeCombinations(productVariantId);
            var productVariantAttributesModel = productVariantAttributeCombinations
                .Select(x =>
                {
                    var pvacModel = new ProductVariantModel.ProductVariantAttributeCombinationModel()
                    {
                        Id = x.Id,
                        ProductVariantId = x.ProductVariantId,
                        AttributesXml = _productAttributeFormatter.FormatAttributes(x.ProductVariant, 
                            x.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false),
                        StockQuantity1 = x.StockQuantity,
                        AllowOutOfStockOrders1 = x.AllowOutOfStockOrders,
                        Sku1 = x.Sku,
                        ManufacturerPartNumber1 = x.ManufacturerPartNumber,
                        Gtin1 = x.Gtin
                    };
                    //warnings
                    var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(ShoppingCartType.ShoppingCart,
                            x.ProductVariant, x.AttributesXml);
                    for (int i = 0; i < warnings.Count;i++ )
                    {
                        pvacModel.Warnings +=warnings[i];
                        if (i != warnings.Count - 1)
                            pvacModel.Warnings += "<br />";
                    }

                    return pvacModel;
                })
                .ToList();

            var model = new GridModel<ProductVariantModel.ProductVariantAttributeCombinationModel>
            {
                Data = productVariantAttributesModel,
                Total = productVariantAttributesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttrbiuteCombinationUpdate(GridCommand command, ProductVariantModel.ProductVariantAttributeCombinationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pvac = _productAttributeService.GetProductVariantAttributeCombinationById(model.Id);
            if (pvac == null)
                throw new ArgumentException("No product variant attribute combination found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = _productService.GetProductVariantById(pvac.ProductVariantId);
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            pvac.StockQuantity = model.StockQuantity1;
            pvac.AllowOutOfStockOrders = model.AllowOutOfStockOrders1;
            pvac.Sku = model.Sku1;
            pvac.ManufacturerPartNumber = model.ManufacturerPartNumber1;
            pvac.Gtin = model.Gtin1;
            _productAttributeService.UpdateProductVariantAttributeCombination(pvac);

            return ProductVariantAttributeCombinationList(command, pvac.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeCombinationDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var pvac = _productAttributeService.GetProductVariantAttributeCombinationById(id);
            if (pvac == null)
                throw new ArgumentException("No product variant attribute combination found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var productVariant = _productService.GetProductVariantById(pvac.ProductVariantId);
                var product = _productService.GetProductById(productVariant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productVariantId = pvac.ProductVariantId;
            _productAttributeService.DeleteProductVariantAttributeCombination(pvac);

            return ProductVariantAttributeCombinationList(command, productVariantId);
        }
        
        //edit
        public ActionResult AddAttributeCombinationPopup(string btnId, string formId, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(productVariantId);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(variant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            var model = new AddProductVariantAttributeCombinationModel();
            PrepareAddProductAttributeCombinationModel(model, variant);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddAttributeCombinationPopup(string btnId, string formId, int productVariantId, 
            AddProductVariantAttributeCombinationModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(productVariantId);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(variant.ProductId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List", "Product");
                }
            }

            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            //attributes
            string attributes = "";
            var warnings = new List<string>();

            #region Product attributes
            string selectedAttributes = string.Empty;
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(variant.Id);
            foreach (var attribute in productVariantAttributes)
            {
                string controlId = string.Format("product_attribute_{0}_{1}", attribute.ProductAttributeId, attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var httpPostedFile = this.Request.Files[controlId];
                            if ((httpPostedFile != null) && (!String.IsNullOrEmpty(httpPostedFile.FileName)))
                            {
                                int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
                                if (httpPostedFile.ContentLength > fileMaxSize)
                                {
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
                                }
                                else
                                {
                                    //save an uploaded file
                                    var download = new Download()
                                    {
                                        DownloadGuid = Guid.NewGuid(),
                                        UseDownloadUrl = false,
                                        DownloadUrl = "",
                                        DownloadBinary = httpPostedFile.GetDownloadBits(),
                                        ContentType = httpPostedFile.ContentType,
                                        Filename = System.IO.Path.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                        Extension = System.IO.Path.GetExtension(httpPostedFile.FileName),
                                        IsNew = true
                                    };
                                    _downloadService.InsertDownload(download);
                                    //save attribute
                                    selectedAttributes = _productAttributeParser.AddProductAttribute(selectedAttributes,
                                        attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            attributes = selectedAttributes;

            #endregion

            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(ShoppingCartType.ShoppingCart,
                variant, attributes));
            if (warnings.Count == 0)
            {
                //save combination
                var combination = new ProductVariantAttributeCombination()
                {
                    ProductVariantId = variant.Id,
                    AttributesXml = attributes,
                    StockQuantity = model.StockQuantity,
                    AllowOutOfStockOrders = model.AllowOutOfStockOrders,
                    Sku = model.Sku,
                    ManufacturerPartNumber = model.ManufacturerPartNumber,
                    Gtin = model.Gtin,
                };
                _productAttributeService.InsertProductVariantAttributeCombination(combination);

                ViewBag.RefreshPage = true;
                return View(model);
            }
            else
            {
                //If we got this far, something failed, redisplay form
                PrepareAddProductAttributeCombinationModel(model, variant);
                model.Warnings = warnings;
                return View(model);
            }
        }
        
        #endregion

    }
}
