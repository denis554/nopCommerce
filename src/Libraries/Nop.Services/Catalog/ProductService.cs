
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Core;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        #region Constants
        private const string PRODUCTS_BY_ID_KEY = "Nop.product.id-{0}";
        private const string PRODUCTVARIANTS_ALL_KEY = "Nop.productvariant.all-{0}-{1}";
        private const string PRODUCTVARIANTS_BY_ID_KEY = "Nop.productvariant.id-{0}";
        private const string TIERPRICES_ALLBYPRODUCTVARIANTID_KEY = "Nop.tierprice.allbyproductvariantid-{0}";
        private const string PRODUCTS_PATTERN_KEY = "Nop.product.";
        private const string PRODUCTVARIANTS_PATTERN_KEY = "Nop.productvariant.";
        private const string TIERPRICES_PATTERN_KEY = "Nop.tierprice.";
        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICacheManager _cacheManager;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="productCategoryRepository">Product category repository</param>
        /// <param name="productManufacturerRepository">Product manufacturer repository</param>
        /// <param name="productVariantRepository">Product variant repository</param>
        /// <param name="relatedProductRepository">Related product repository</param>
        /// <param name="crossSellProductRepository">Cross-sell product repository</param>
        /// <param name="tierPriceRepository">Tier price repository</param>
        /// <param name="productAttributeService">Product attribute service</param>
        /// <param name="productAttributeParser">Product attribute parser service</param>
        /// <param name="localizationSettings">Localization settings</param>
        public ProductService(ICacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<TierPrice> tierPriceRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            LocalizationSettings localizationSettings)
        {
            this._cacheManager = cacheManager;
            this._productRepository = productRepository;
            this._productCategoryRepository = productCategoryRepository;
            this._productManufacturerRepository = productManufacturerRepository;
            this._productVariantRepository = productVariantRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._crossSellProductRepository = crossSellProductRepository;
            this._tierPriceRepository = tierPriceRepository;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._localizationSettings = localizationSettings;
        }

        #endregion

        #region Methods

        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        public void DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            product.Deleted = true;
            //delete product
            UpdateProduct(product);

            //delete product variants
            foreach (var productVariant in product.ProductVariants)
                DeleteProductVariant(productVariant);
        }

        /// <summary>
        /// Gets all products
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection</returns>
        public IList<Product> GetAllProducts(bool showHidden = false)
        {
            var query = from p in _productRepository.Table
                        orderby p.Name
                        where (showHidden || p.Published) &&
                        !p.Deleted
                        select p;
            var products = query.ToList();
            return products;
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Product collection</returns>
        public IList<Product> GetAllProductsDisplayedOnHomePage()
        {
            var query = from p in _productRepository.Table
                        orderby p.Name
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowOnHomePage
                        select p;
            var products = query.ToList();
            return products;
        }
        
        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            string key = string.Format(PRODUCTS_BY_ID_KEY, productId);
            return _cacheManager.Get(key, () =>
            {
                var product = _productRepository.GetById(productId);
                return product;
            });
        }
        
        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        public void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //TODO:Check and make sure other services are settings this value on insert.
            product.CreatedOnUtc = DateTime.UtcNow;
            //TODO:Also check and make sure the updated on property is being set in the services
            product.UpdatedOnUtc = DateTime.UtcNow;

            _productRepository.Insert(product);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            _productRepository.Update(product);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }
         
        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all recordss</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="relatedToProductId">Filter by related product; 0 to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search in descriptions</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="filteredSpecs">Filtered product specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection</returns>
        public IPagedList<Product> SearchProducts(int categoryId, int manufacturerId, bool? featuredProducts,
            decimal? priceMin, decimal? priceMax,
            int relatedToProductId, int productTagId,
            string keywords, bool searchDescriptions, int languageId,
            IList<int> filteredSpecs, ProductSortingEnum orderBy,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            //UNDONE temporary solution (requires optimization)

            var allProducts = GetAllProducts(showHidden);
            var filteredProducts = new List<Product>();
            foreach (var prod in allProducts)
            {
                var productVariants = prod.ProductVariants.Where(pv => (showHidden || !pv.Deleted) && (showHidden || pv.Published)).ToList();

                //filter by category
                bool categoryOK = false;
                if (categoryId > 0)
                {
                    var pc = prod.ProductCategories.FirstOrDefault(pc1 => pc1.CategoryId == categoryId);
                    if (pc != null)
                    {
                        if (featuredProducts.HasValue)
                        {
                            categoryOK = featuredProducts.Value == pc.IsFeaturedProduct;
                        }
                        else
                        {
                            categoryOK = true;
                        }
                    }
                }
                else
                    categoryOK = true;

                //filter by manufacturer
                bool manufacturerOK = false;
                if (manufacturerId > 0)
                {
                    var pm = prod.ProductManufacturers.FirstOrDefault(pm1 => pm1.ManufacturerId == manufacturerId);
                    if (pm != null)
                    {
                        if (featuredProducts.HasValue)
                        {
                            manufacturerOK = featuredProducts.Value == pm.IsFeaturedProduct;
                        }
                        else
                        {
                            manufacturerOK = true;
                        }
                    }
                }
                else
                    manufacturerOK = true;

                //filter by price
                bool priceMinOK = false;
                if (priceMin.HasValue)
                {
                    foreach (var pv in productVariants)
                    {
                        if (pv.Price > priceMin.Value)
                        {
                            priceMinOK = true;
                            break;
                        }
                    }
                }
                else
                    priceMinOK = true;
                bool priceMaxOK = false;
                if (priceMax.HasValue)
                {
                    foreach (var pv in productVariants)
                    {
                        if (pv.Price < priceMax.Value)
                        {
                            priceMaxOK = true;
                            break;
                        }
                    }
                }
                else
                    priceMaxOK = true;


                //filter by related products
                bool relatedProductOK = false;
                if (relatedToProductId > 0)
                {
                    var relatedProducts = GetRelatedProductsByProductId1(prod.Id, showHidden);
                    foreach (var rp in relatedProducts)
                    {
                        if (rp.Id == relatedToProductId)
                        {
                            relatedProductOK = true;
                            break;
                        }
                    }
                }
                else
                    relatedProductOK = true;


                //filter by product tags
                bool productTagOK = false; 
                if (productTagId > 0)
                {
                    //UNDONE use productTagId parameter
                    productTagOK = true;
                }
                else
                    productTagOK = true;

                //filter by keywords
                bool keywordsOK = false;
                if (!String.IsNullOrWhiteSpace(keywords))
                {
                    keywords = keywords.ToLowerInvariant();
                    //UNDONE search localized values (languageId parameter)
                    if (!String.IsNullOrEmpty(prod.Name) && prod.Name.ToLowerInvariant().Contains(keywords))
                    {
                        keywordsOK = true;
                    }

                    if (!keywordsOK)
                    {
                        if (searchDescriptions)
                        {
                            if (!String.IsNullOrEmpty(prod.ShortDescription) && prod.ShortDescription.ToLowerInvariant().Contains(keywords))
                            {
                                keywordsOK = true;
                            }

                            if (!String.IsNullOrEmpty(prod.FullDescription) && prod.FullDescription.ToLowerInvariant().Contains(keywords))
                            {
                                keywordsOK = true;
                            }
                        }
                    }

                    if (!keywordsOK)
                    {
                        foreach (var pv in productVariants)
                        {
                            if (!String.IsNullOrEmpty(pv.Name) && pv.Name.ToLowerInvariant().Contains(keywords))
                            {
                                keywordsOK = true;
                                break;
                            }
                        }
                    }

                    if (!keywordsOK)
                    {
                        if (searchDescriptions)
                        {
                            foreach (var pv in productVariants)
                            {
                                if (!String.IsNullOrEmpty(pv.Description) && pv.Description.ToLowerInvariant().Contains(keywords))
                                {
                                    keywordsOK = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                    keywordsOK = true;

                //filter by specs
                bool specificationsOK = true;
                if (filteredSpecs != null && filteredSpecs.Count > 0)
                {
                    for (int i = 0; i < filteredSpecs.Count; i++)
                    {
                        bool specIsFound = false;
                        foreach (var psa in prod.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering))
                            if (psa.SpecificationAttributeOptionId == filteredSpecs[i])
                            {
                                specIsFound = true;
                                break;
                            }
                        if (!specIsFound)
                        {
                            specificationsOK = false;
                            break;
                        }
                    }
                }
                else
                    specificationsOK = true;

                if (categoryOK && manufacturerOK && priceMinOK && priceMaxOK 
                    && relatedProductOK && productTagOK && specificationsOK && keywordsOK)
                    filteredProducts.Add(prod);
            }

            //sort products
            var sortedProducts = new List<Product>();
            if (orderBy == ProductSortingEnum.Position && categoryId > 0)
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.ProductCategories.First().DisplayOrder).ToList();
            else if (orderBy == ProductSortingEnum.Position && manufacturerId > 0)
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.ProductManufacturers.First().DisplayOrder).ToList();
            else if (orderBy == ProductSortingEnum.Position && relatedToProductId > 0)
            {
                //sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.RelatedProducts.First().DisplayOrder).ToList();
                //UNDONE sort by related product ID
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.Name).ToList();
            }
            else if (orderBy == ProductSortingEnum.Position)
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.Name).ToList();
            else if (orderBy == ProductSortingEnum.Name)
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.Name).ToList();
            else if (orderBy == ProductSortingEnum.Price)
                sortedProducts = filteredProducts.AsQueryable().OrderBy(p => p.ProductVariants.First().Price).ToList();
            else if (orderBy == ProductSortingEnum.CreatedOn)
                sortedProducts = filteredProducts.AsQueryable().OrderByDescending(p => p.CreatedOnUtc).ToList();
            
            var products = new PagedList<Product>(sortedProducts, pageIndex, pageSize);
            return products;
        }
        #endregion

        #region Product variants
        
        /// <summary>
        /// Get low stock product variants
        /// </summary>
        /// <returns>Result</returns>
        public IList<ProductVariant> GetLowStockProductVariants()
        {
            var query = from pv in _productVariantRepository.Table
                        orderby pv.MinStockQuantity
                        where !pv.Deleted &&
                        pv.MinStockQuantity >= pv.StockQuantity
                        select pv;
            var productVariants = query.ToList();
            return productVariants;
        }
        
        /// <summary>
        /// Gets a product variant
        /// </summary>
        /// <param name="productVariantId">Product variant identifier</param>
        /// <returns>Product variant</returns>
        public ProductVariant GetProductVariantById(int productVariantId)
        {
            if (productVariantId == 0)
                return null;

            string key = string.Format(PRODUCTVARIANTS_BY_ID_KEY, productVariantId);
            return _cacheManager.Get(key, () =>
            {
                var pv = _productVariantRepository.GetById(productVariantId);
                return pv;
            });
        }

        /// <summary>
        /// Gets a product variant by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product variant</returns>
        public ProductVariant GetProductVariantBySku(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from pv in _productVariantRepository.Table
                        orderby pv.DisplayOrder, pv.Id
                        where !pv.Deleted &&
                        pv.Sku == sku
                        select pv;
            var productVariant = query.FirstOrDefault();
            return productVariant;
        }
        
        /// <summary>
        /// Inserts a product variant
        /// </summary>
        /// <param name="productVariant">The product variant</param>
        public void InsertProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            _productVariantRepository.Insert(productVariant);
            
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the product variant
        /// </summary>
        /// <param name="productVariant">The product variant</param>
        public void UpdateProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            _productVariantRepository.Update(productVariant);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }
        
        /// <summary>
        /// Gets product variants by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variant collection</returns>
        public IList<ProductVariant> GetProductVariantsByProductId(int productId, bool showHidden = false)
        {
            string key = string.Format(PRODUCTVARIANTS_ALL_KEY, showHidden, productId);
            return _cacheManager.Get(key, () =>
            {
                var query = (IQueryable<ProductVariant>)_productVariantRepository.Table;
                if (!showHidden)
                {
                    query = query.Where(pv => pv.Published);
                }
                if (!showHidden)
                {
                    query = query.Where(
                            pv =>
                            !pv.AvailableStartDateTimeUtc.HasValue ||
                            pv.AvailableStartDateTimeUtc <= DateTime.UtcNow);
                    query = query.Where(
                            pv =>
                            !pv.AvailableEndDateTimeUtc.HasValue ||
                            pv.AvailableEndDateTimeUtc >= DateTime.UtcNow);
                }
                query = query.Where(pv => !pv.Deleted);
                query = query.Where(pv => pv.ProductId == productId);
                query = query.OrderBy(pv => pv.DisplayOrder);

                var productVariants = query.ToList();
                return productVariants;
            });
        }

        /// <summary>
        /// Delete a product variant
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        public void DeleteProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            productVariant.Deleted = true;
            UpdateProductVariant(productVariant);
        }


        /// <summary>
        /// Adjusts inventory
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="decrease">A value indicating whether to increase or descrease product variant stock quantity</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        public void AdjustInventory(ProductVariant productVariant, bool decrease,
            int quantity, string attributesXml)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                        //do nothing
                        return;
                    }
                case ManageInventoryMethod.ManageStock:
                    {
                        int newStockQuantity = 0;
                        if (decrease)
                            newStockQuantity = productVariant.StockQuantity - quantity;
                        else
                            newStockQuantity = productVariant.StockQuantity + quantity;

                        bool newPublished = productVariant.Published;
                        bool newDisableBuyButton = productVariant.DisableBuyButton;

                        //check if minimum quantity is reached
                        if (decrease)
                        {
                            if (productVariant.MinStockQuantity >= newStockQuantity)
                            {
                                switch (productVariant.LowStockActivity)
                                {
                                    case LowStockActivity.DisableBuyButton:
                                        newDisableBuyButton = true;
                                        break;
                                    case LowStockActivity.Unpublish:
                                        newPublished = false;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        if (decrease && productVariant.NotifyAdminForQuantityBelow > newStockQuantity)
                        {
                            //UNDONE send email notification
                            //_messageService.SendQuantityBelowStoreOwnerNotification(productVariant, _localizationSettings.DefaultAdminLanguageId);
                        }

                        productVariant.StockQuantity = newStockQuantity;
                        productVariant.DisableBuyButton = newDisableBuyButton;
                        productVariant.Published = newPublished;
                        UpdateProductVariant(productVariant);

                        if (decrease)
                        {
                            var product = productVariant.Product;
                            bool allProductVariantsUnpublished = true;
                            foreach (var pv2 in product.ProductVariants)
                            {
                                if (pv2.Published)
                                {
                                    allProductVariantsUnpublished = false;
                                    break;
                                }
                            }

                            if (allProductVariantsUnpublished)
                            {
                                product.Published = false;
                                UpdateProduct(product);
                            }
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combination = _productAttributeParser.FindProductVariantAttributeCombination(productVariant, attributesXml);
                        if (combination != null)
                        {
                            int newStockQuantity = 0;
                            if (decrease)
                                newStockQuantity = combination.StockQuantity - quantity;
                            else
                                newStockQuantity = combination.StockQuantity + quantity;

                            combination.StockQuantity = newStockQuantity;
                            _productAttributeService.UpdateProductVariantAttributeCombination(combination);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Delete(relatedProduct);
        }

        /// <summary>
        /// Gets a related product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Related product collection</returns>
        public IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from rp in _relatedProductRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId1 &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby rp.DisplayOrder
                        select rp;
            var relatedProducts = query.ToList();

            return relatedProducts;
        }

        /// <summary>
        /// Gets a related product
        /// </summary>
        /// <param name="relatedProductId">Related product identifer</param>
        /// <returns>Related product</returns>
        public RelatedProduct GetRelatedProductById(int relatedProductId)
        {
            if (relatedProductId == 0)
                return null;
            
            var relatedProduct = _relatedProductRepository.GetById(relatedProductId);
            return relatedProduct;
        }

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public void InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Insert(relatedProduct);
        }

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Update(relatedProduct);
        }

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell intifer</param>
        public void DeleteCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Delete(crossSellProduct);
        }

        /// <summary>
        /// Gets a cross-sell product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Cross-sell product collection</returns>
        public IList<CrossSellProduct> GetCrossSellProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from csp in _crossSellProductRepository.Table
                        join p in _productRepository.Table on csp.ProductId2 equals p.Id
                        where csp.ProductId1 == productId1 &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby csp.Id
                        select csp;
            var crossSellProducts = query.ToList();
            return crossSellProducts;
        }

        /// <summary>
        /// Gets a cross-sell product
        /// </summary>
        /// <param name="crossSellProductId">Cross-sell product identifer</param>
        /// <returns>Cross-sell product</returns>
        public CrossSellProduct GetCrossSellProductById(int crossSellProductId)
        {
            if (crossSellProductId == 0)
                return null;

            var crossSellProduct = _crossSellProductRepository.GetById(crossSellProductId);
            return crossSellProduct;
        }

        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public void InsertCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Insert(crossSellProduct);
        }

        /// <summary>
        /// Updates a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public void UpdateCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Update(crossSellProduct);
        }

        #endregion
        
        #region Tier prices
        
        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public void DeleteTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Delete(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="tierPriceId">Tier price identifier</param>
        /// <returns>Tier price</returns>
        public TierPrice GetTierPriceById(int tierPriceId)
        {
            if (tierPriceId == 0)
                return null;
            
            var tierPrice = _tierPriceRepository.GetById(tierPriceId);
            return tierPrice;
        }

        /// <summary>
        /// Gets tier prices by product variant identifier
        /// </summary>
        /// <param name="productVariantId">Product variant identifier</param>
        /// <returns>Tier price collection</returns>
        public IList<TierPrice> GetTierPricesByProductVariantId(int productVariantId)
        {
            if (productVariantId == 0)
                return new List<TierPrice>();

            string key = string.Format(TIERPRICES_ALLBYPRODUCTVARIANTID_KEY, productVariantId);
            return _cacheManager.Get(key, () =>
            {
                var query = from tp in _tierPriceRepository.Table
                            orderby tp.Quantity
                            where tp.ProductVariantId == productVariantId
                            select tp;
                var tierPrices = query.ToList();
                return tierPrices;
            });
        }

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public void InsertTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Insert(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public void UpdateTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Update(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
        }

        #endregion

        #endregion
    }
}
