﻿using System;
using System.Linq;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Localization;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Working context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        private const string CustomerCookieName = "Nop.customer";

        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWebHelper _webHelper;

        private Store _cachedStore;

        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated;
        private bool _cachedIsAdmin;

        public WebWorkContext(HttpContextBase httpContext,
            ICustomerService customerService,
            IStoreService storeService,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            TaxSettings taxSettings, CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            IWebHelper webHelper)
        {
            this._httpContext = httpContext;
            this._customerService = customerService;
            this._storeService = storeService;
            this._authenticationService = authenticationService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._localizationSettings = localizationSettings;
            this._webHelper = webHelper;
        }

        protected Customer GetCurrentCustomer()
        {
            if (_cachedCustomer != null)
                return _cachedCustomer;

            Customer customer = null;
            if (_httpContext != null)
            {
                //check whether request is made by a search engine
                //in this case return built-in customer record for search engines 
                //or comment the following two lines of code in order to disable this functionality
                if (_webHelper.IsSearchEngine(_httpContext))
                    customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);

                //registered user
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    customer = _authenticationService.GetAuthenticatedCustomer();
                }

                //impersonate user if required (currently used for 'phone order' support)
                if (customer != null && !customer.Deleted && customer.Active)
                {
                        int? impersonatedCustomerId = customer.GetAttribute<int?>(SystemCustomerAttributeNames.ImpersonatedCustomerId);
                        if (impersonatedCustomerId.HasValue && impersonatedCustomerId.Value > 0)
                        {
                            var impersonatedCustomer = _customerService.GetCustomerById(impersonatedCustomerId.Value);
                            if (impersonatedCustomer != null && !impersonatedCustomer.Deleted && impersonatedCustomer.Active)
                            {
                                //set impersonated customer
                                _originalCustomerIfImpersonated = customer;
                                customer = impersonatedCustomer;
                            }
                        }
                }

                //load guest customer
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    var customerCookie = GetCustomerCookie();
                    if (customerCookie != null && !String.IsNullOrEmpty(customerCookie.Value))
                    {
                        Guid customerGuid;
                        if (Guid.TryParse(customerCookie.Value, out customerGuid))
                        {
                            var customerByCookie = _customerService.GetCustomerByGuid(customerGuid);
                            if (customerByCookie != null &&
                                //this customer (from cookie) should not be registered
                                !customerByCookie.IsRegistered() &&
                                //it should not be a built-in 'search engine' customer account
                                !customerByCookie.IsSearchEngineAccount())
                                customer = customerByCookie;
                        }
                    }
                }

                //create guest if not exists
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    customer = _customerService.InsertGuestCustomer();
                }

                SetCustomerCookie(customer.CustomerGuid);
            }

            //validation
            if (customer != null && !customer.Deleted && customer.Active)
            {

                _cachedCustomer = customer;
            }

            return _cachedCustomer;
        }

        protected HttpCookie GetCustomerCookie()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            return _httpContext.Request.Cookies[CustomerCookieName];
        }

        protected void SetCustomerCookie(Guid customerGuid)
        {
            var cookie = new HttpCookie(CustomerCookieName);
            cookie.HttpOnly = true;
            cookie.Value = customerGuid.ToString();
            if (customerGuid == Guid.Empty)
            {
                cookie.Expires = DateTime.Now.AddMonths(-1);
            }
            else
            {
                int cookieExpires = 24 * 365; //TODO make configurable
                cookie.Expires = DateTime.Now.AddHours(cookieExpires);
            }
            if (_httpContext != null && _httpContext.Response != null)
            {
                _httpContext.Response.Cookies.Remove(CustomerCookieName);
                _httpContext.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        public virtual Store CurrentStore
        {
            get
            {
                if (_cachedStore != null)
                    return _cachedStore;

                Store store = null;
                if (_httpContext != null)
                {
                    //TODO determine the current store by HTTP_HOST
                }

                if (store == null)
                {
                    //load the first found store
                    store = _storeService.GetAllStores().FirstOrDefault();
                }

                _cachedStore = store;
                return _cachedStore;
            }
        }

        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        public virtual Customer CurrentCustomer
        {
            get
            {
                return GetCurrentCustomer();
            }
            set
            {
                SetCustomerCookie(value.CustomerGuid);
                _cachedCustomer = value;
            }
        }

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated)
        /// </summary>
        public virtual Customer OriginalCustomerIfImpersonated
        {
            get
            {
                return _originalCustomerIfImpersonated;
            }
        }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        public virtual Language WorkingLanguage
        {
            get
            {
                //get language from URL (if possible)
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    if (_httpContext != null)
                    {
                        string virtualPath = _httpContext.Request.AppRelativeCurrentExecutionFilePath;
                        string applicationPath = _httpContext.Request.ApplicationPath;
                        if (virtualPath.IsLocalizedUrl(applicationPath, false))
                        {
                            var seoCode = virtualPath.GetLanguageSeoCodeFromUrl(applicationPath, false);
                            if (!String.IsNullOrEmpty(seoCode))
                            {
                                var langByCulture = _languageService.GetAllLanguages()
                                    .FirstOrDefault(l => seoCode.Equals(l.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase));
                                if (langByCulture != null && langByCulture.Published)
                                {
                                    //the language is found. now we need to save it
                                    if (this.CurrentCustomer != null && !langByCulture.Equals(this.CurrentCustomer.Language))
                                    {
                                        this.CurrentCustomer.Language = langByCulture;
                                        _customerService.UpdateCustomer(this.CurrentCustomer);
                                    }
                                }
                            }
                        }
                    }
                }
                var allLanguages = _languageService.GetAllLanguages(storeId: this.CurrentStore.Id);
                if (allLanguages.Count > 0)
                {
                    //find current customer language
                    foreach (var lang in allLanguages)
                    {
                        if (this.CurrentCustomer != null && this.CurrentCustomer.LanguageId == lang.Id)
                        {
                            return lang;
                        }
                    }
                    //it not specified, then return the first found one
                    return allLanguages.FirstOrDefault();
                }

                //if not found in languaged filtered by the current store, then return any language
                return _languageService.GetAllLanguages().FirstOrDefault();
            }
            set
            {
                if (this.CurrentCustomer == null)
                    return;

                this.CurrentCustomer.Language = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        public virtual Currency WorkingCurrency
        {
            get
            {
                //return primary store currency when we're in admin area/mode
                if (this.IsAdmin)
                {
                    var primaryStoreCurrency =  _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    if (primaryStoreCurrency != null)
                        return primaryStoreCurrency;
                }

                if (this.CurrentCustomer != null &&
                    this.CurrentCustomer.Currency != null &&
                    this.CurrentCustomer.Currency.Published)
                    return this.CurrentCustomer.Currency;

                var currency = _currencyService.GetAllCurrencies().FirstOrDefault();
                return currency;
            }
            set
            {
                if (this.CurrentCustomer == null)
                    return;

                this.CurrentCustomer.Currency = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public virtual TaxDisplayType TaxDisplayType
        {
            get
            {
                if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                {
                    if (this.CurrentCustomer != null)
                        return this.CurrentCustomer.TaxDisplayType;
                }

                return _taxSettings.TaxDisplayType;
            }
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    return;

                this.CurrentCustomer.TaxDisplayType = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        public virtual bool IsAdmin
        {
            get
            {
                return _cachedIsAdmin;
            }
            set
            {
                _cachedIsAdmin = value;
            }
        }
    }
}
