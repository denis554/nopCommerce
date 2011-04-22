﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Models;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace Nop.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductController : BaseNopController
    {
		#region Fields

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;

        #endregion Fields 

		#region Constructors

        public ProductController(IProductService productService, 
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IWorkContext workContext, ILanguageService languageService, 
            ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._specificationAttributeService = specificationAttributeService;
        }

        #endregion Constructors 

        #region Utitilies

        [NonAction]
        private void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.ShortDescription,
                                                               localized.ShortDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.FullDescription,
                                                               localized.FullDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaKeywords,
                                                               localized.MetaKeywords,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaDescription,
                                                               localized.MetaDescription,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.MetaTitle,
                                                               localized.MetaTitle,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                                                               x => x.SeName,
                                                               localized.SeName,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        private string GetCategoryFullName(Category category)
        {
            string result = string.Empty;

            while (category != null)
            {
                if (String.IsNullOrEmpty(result))
                    result = category.Name;
                else
                    result = "--" + result;
                category = _categoryService.GetCategoryById(category.ParentCategoryId);
            }
            return result;
        }

        private void PrepareAddSpecificationAttributeModel(ProductModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.AddSpecificationAttributeModel == null)
                model.AddSpecificationAttributeModel = new ProductModel.AddProductSpecificationAttributeModel();
            
            //attributes
            var specificationAttributes = _specificationAttributeService.GetSpecificationAttributes();
            for (int i = 0; i < specificationAttributes.Count; i++)
            {
                var sa = specificationAttributes[i];
                model.AddSpecificationAttributeModel.AvailableAttributes.Add(new SelectListItem() { Text = sa.Name, Value = sa.Id.ToString() });
                if (i == 0)
                {
                    //attribute options
                    foreach (var sao in sa.SpecificationAttributeOptions)
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem() { Text = sao.Name, Value = sao.Id.ToString() });
                }
            }
            //UNDONE: display specification attributes without options?

        }
        #endregion

        #region Methods

        //list products
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            var products = _productService.SearchProducts(0, 0, null, null, null, 0, 0, string.Empty, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, 10, true);

            var model = new ProductListModel();
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(true))
                model.AvailableCategories.Add(new SelectListItem() { Text = GetCategoryFullName(c), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command)
        {
            //filtering
            string productName = command.FilterDescriptors.GetValueFromAppliedFilters("Name", FilterOperator.Contains);
            string selectedCategoryId = command.FilterDescriptors.GetValueFromAppliedFilters("SearchCategoryId");
            string selectedManufacturerId = command.FilterDescriptors.GetValueFromAppliedFilters("SearchManufacturerId");
            
            var model = new GridModel();
            var products = _productService.SearchProducts(!String.IsNullOrEmpty(selectedCategoryId) ? Convert.ToInt32(selectedCategoryId) : 0,
                !String.IsNullOrEmpty(selectedManufacturerId) ? Convert.ToInt32(selectedManufacturerId) : 0
                , null, null, null, 0, 0, productName, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, command.Page - 1, command.PageSize, true);
            model.Data = products.Select(x => x.ToModel());
            model.Total = products.TotalCount;
            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("search-products")]
        public ActionResult Search(ProductListModel model)
        {
            ViewData["searchProductName"] = model.SearchProductName;
            ViewData["searchCategoryId"] = model.SearchCategoryId;
            ViewData["searchManufacturerId"] = model.SearchManufacturerId;

            var products = _productService.SearchProducts(model.SearchCategoryId, 
                model.SearchManufacturerId, null, null, null, 0, 0, model.SearchProductName, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, 10, true);

            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(true))
                model.AvailableCategories.Add(new SelectListItem() { Text = GetCategoryFullName(c), Value = c.Id.ToString(), Selected = c.Id == model.SearchCategoryId });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString(), Selected = m.Id == model.SearchManufacturerId });

            return View(model);
        }
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public ActionResult GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;
            var pv = _productService.GetProductVariantBySku(sku);
            if (pv != null)
                return RedirectToAction("Edit", "ProductVariant", new { id = pv.Id });
            
            var products = _productService.SearchProducts(0,
                0, null, null, null, 0, 0, string.Empty, false,
                _workContext.WorkingLanguage.Id, new List<int>(),
                ProductSortingEnum.Position, 0, 10, true);

            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(true))
                model.AvailableCategories.Add(new SelectListItem() { Text = GetCategoryFullName(c), Value = c.Id.ToString(), Selected = c.Id == model.SearchCategoryId });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString(), Selected = m.Id == model.SearchManufacturerId });

            return View(model);
        }

        [HttpPost]
        public ActionResult _AjaxComboBo(string text)
        {
            var products = _productService.GetAllProducts(true).Where(x => x.Name.ToLower().Contains(text.ToLower()));

            return new JsonResult
            {
                Data = new SelectList(products.ToList(), "Id",
                    "Name")
            };
        }

        //create product
        public ActionResult Create()
        {
            var model = new ProductModel();
            AddLocales(_languageService, model.Locales);
            PrepareAddSpecificationAttributeModel(model);
            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Create(ProductModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var product = model.ToEntity();
                _productService.InsertProduct(product);
                UpdateLocales(product, model);

                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAddSpecificationAttributeModel(model);
            return View(model);
        }

        //edit product
        public ActionResult Edit(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                throw new ArgumentException("No category found with the specified id", "id");

            var model = product.ToModel();
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
                {
                    locale.Name = product.GetLocalized(x => x.Name, languageId, false);
                    locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false);
                    locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false);
                    locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false);
                    locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false);
                    locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false);
                    locale.SeName = product.GetLocalized(x => x.SeName, languageId, false);
                });
            PrepareAddSpecificationAttributeModel(model);
            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Edit(ProductModel model, bool continueEditing)
        {
            var product = _productService.GetProductById(model.Id);
            if (product == null) 
                throw new ArgumentException("No category found with the specified id");
            
            if (ModelState.IsValid)
            {
                product = model.ToEntity(product);
                _productService.UpdateProduct(product);
                UpdateLocales(product, model);
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareAddSpecificationAttributeModel(model);
            return View(model);
        }

        //delete product
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = _productService.GetProductById(id);
            _productService.DeleteProduct(product);
            return RedirectToAction("List");
        }
        

        //product specification attributes
        public ActionResult ProductSpecificationAttributeAdd(int specificationAttributeOptionId, 
            bool allowFiltering, bool showOnProductPage, int displayOrder, int productId)
        {
            var psa = new ProductSpecificationAttribute()
            {
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                ProductId = productId,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };
            _specificationAttributeService.InsertProductSpecificationAttribute(psa);

            return Json(new { Result = "1" }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrList(GridCommand command, int productId)
        {
            var productrSpecs = _specificationAttributeService.GetProductSpecificationAttributesByProductId(productId);

            var productrSpecsModel = productrSpecs
                .Select(x =>
                {
                    var psaModel = new ProductSpecificationAttributeModel()
                    {
                        Id = x.Id,
                        SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOption.Name,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnProductPage = x.ShowOnProductPage,
                        DisplayOrder = x.DisplayOrder
                    };
                    return psaModel;
                })
                .ToList();

            var model = new GridModel<ProductSpecificationAttributeModel>
            {
                Data = productrSpecsModel,
                Total = productrSpecsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrUpdate(int psaId, ProductSpecificationAttributeModel model,
            GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                //TODO:Find out how telerik handles errors
                return new JsonResult { Data = "error" };
            }

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            psa.AllowFiltering = model.AllowFiltering;
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, psa.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductSpecAttrDelete(int psaId, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                //TODO:Find out how telerik handles errors
                return new JsonResult { Data = "error" };
            }

            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            var productId = psa.ProductId;
            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, productId);
        }


		#endregion
    }
}
