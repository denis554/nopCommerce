﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Admin.Models;
using Nop.Admin.Models.Common;
using Nop.Admin.Models.Forums;
using Nop.Admin.Models.Settings;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Blogs;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;

namespace Nop.Admin.Controllers
{
	[AdminAuthorize]
    public class SettingController : BaseNopController
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IEncryptionService _encryptionService;

        private BlogSettings _blogSettings;
        private ForumSettings _forumSettings;
        private NewsSettings _newsSettings;
        private ShippingSettings _shippingSettings;
        private TaxSettings _taxSettings;
        private CatalogSettings _catalogSettings;
        private RewardPointsSettings _rewardPointsSettings;
        private readonly CurrencySettings _currencySettings;
        private OrderSettings _orderSettings;
        private ShoppingCartSettings _shoppingCartSettings;
        private MediaSettings _mediaSettings;
        private CustomerSettings _customerSettings;
        private UserSettings _userSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly SeoSettings _seoSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly PdfSettings _pdfSettings;

		#endregion

		#region Constructors

        public SettingController(ISettingService settingService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IAddressService addressService, ITaxCategoryService taxCategoryService,
            ICurrencyService currencyService, IPictureService pictureService, 
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IOrderService orderService, IUserService userService, 
            IEncryptionService encryptionService, BlogSettings blogSettings,
            ForumSettings forumSettings, NewsSettings newsSettings,
            ShippingSettings shippingSettings, TaxSettings taxSettings,
            CatalogSettings catalogSettings, RewardPointsSettings rewardPointsSettings,
            CurrencySettings currencySettings, OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings, MediaSettings mediaSettings,
            CustomerSettings customerSettings, UserSettings userSettings,
            DateTimeSettings dateTimeSettings, StoreInformationSettings storeInformationSettings,
            SeoSettings seoSettings,SecuritySettings securitySettings, PdfSettings pdfSettings)
        {
            this._settingService = settingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._taxCategoryService = taxCategoryService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._orderService = orderService;
            this._userService = userService;
            this._encryptionService = encryptionService;

            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._newsSettings = newsSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._catalogSettings = catalogSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._currencySettings = currencySettings;
            this._orderSettings = orderSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._mediaSettings = mediaSettings;
            this._customerSettings = customerSettings;
            this._userSettings = userSettings;
            this._dateTimeSettings = dateTimeSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._seoSettings = seoSettings;
            this._securitySettings = securitySettings;
            this._pdfSettings = pdfSettings;
        }

		#endregion Constructors 
        
        #region Methods

        public ActionResult Blog()
        {
            var model = _blogSettings.ToModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Blog(BlogSettingsModel model)
        {
            _blogSettings = model.ToEntity(_blogSettings);
            _settingService.SaveSetting(_blogSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }




        public ActionResult Forum()
        {
            var model = _forumSettings.ToModel();
            model.ForumEditorValues = _forumSettings.ForumEditor.ToSelectList();
            return View(model);
        }
        [HttpPost]
        public ActionResult Forum(ForumSettingsModel model)
        {
            _forumSettings = model.ToEntity(_forumSettings);
            _settingService.SaveSetting(_forumSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }




        public ActionResult News()
        {
            var model = _newsSettings.ToModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult News(NewsSettingsModel model)
        {
            _newsSettings = model.ToEntity(_newsSettings);
            _settingService.SaveSetting(_newsSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }




        public ActionResult Shipping()
        {
            var model = _shippingSettings.ToModel();

            //shipping origin
            var originAddress = _shippingSettings.ShippingOriginAddressId > 0
                                     ? _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId)
                                     : null;
            if (originAddress != null)
                model.ShippingOriginAddress = originAddress.ToModel();
            else
                model.ShippingOriginAddress = new AddressModel();

            model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) });

            var states = originAddress != null && originAddress.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(originAddress.Country.Id, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.ShippingOriginAddress.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == originAddress.StateProvinceId) });
            }
            else
                model.ShippingOriginAddress.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            model.ShippingOriginAddress.FirstNameDisabled = true;
            model.ShippingOriginAddress.LastNameDisabled = true;
            model.ShippingOriginAddress.EmailDisabled = true;
            model.ShippingOriginAddress.CompanyDisabled = true;
            model.ShippingOriginAddress.CityDisabled = true;
            model.ShippingOriginAddress.Address1Disabled = true;
            model.ShippingOriginAddress.Address2Disabled = true;
            model.ShippingOriginAddress.PhoneNumberDisabled = true;
            model.ShippingOriginAddress.FaxNumberDisabled = true;

            return View(model);
        }
        [HttpPost]
        public ActionResult Shipping(ShippingSettingsModel model)
        {
            _shippingSettings = model.ToEntity(_shippingSettings);

            var originAddress = _addressService.GetAddressById(_shippingSettings.ShippingOriginAddressId) ??
                                         new Core.Domain.Common.Address()
                                         {
                                             CreatedOnUtc = DateTime.UtcNow,
                                         };
            originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
            if (originAddress.Id > 0)
                _addressService.UpdateAddress(originAddress);
            else
                _addressService.InsertAddress(originAddress);

            _shippingSettings.ShippingOriginAddressId = originAddress.Id;
            _settingService.SaveSetting(_shippingSettings);


            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }




        public ActionResult Tax()
        {
            var model = _taxSettings.ToModel();
            model.TaxBasedOnValues = _taxSettings.TaxBasedOn.ToSelectList();
            model.TaxDisplayTypeValues = _taxSettings.TaxDisplayType.ToSelectList();

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.ShippingTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.ShippingTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == _taxSettings.ShippingTaxClassId });
            model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" });
            foreach (var tc in taxCategories)
                model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == _taxSettings.PaymentMethodAdditionalFeeTaxClassId });

            //EU VAT countries
            model.EuVatShopCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.EuVatShopCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == _taxSettings.EuVatShopCountryId });

            //default tax address
            var defaultAddress = _taxSettings.DefaultTaxAddressId > 0
                                     ? _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId)
                                     : null;
            if (defaultAddress != null)
                model.DefaultTaxAddress = defaultAddress.ToModel();
            else
                model.DefaultTaxAddress = new AddressModel();

            model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var c in _countryService.GetAllCountries(true))
                model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) });

            var states = defaultAddress != null && defaultAddress.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(defaultAddress.Country.Id, true).ToList() : new List<StateProvince>();
            if (states.Count > 0)
            {
                foreach (var s in states)
                    model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) });
            }
            else
                model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
            model.DefaultTaxAddress.FirstNameDisabled = true;
            model.DefaultTaxAddress.LastNameDisabled = true;
            model.DefaultTaxAddress.EmailDisabled = true;
            model.DefaultTaxAddress.CompanyDisabled = true;
            model.DefaultTaxAddress.CityDisabled = true;
            model.DefaultTaxAddress.Address1Disabled = true;
            model.DefaultTaxAddress.Address2Disabled = true;
            model.DefaultTaxAddress.PhoneNumberDisabled = true;
            model.DefaultTaxAddress.FaxNumberDisabled = true;

            return View(model);
        }
        [HttpPost]
        public ActionResult Tax(TaxSettingsModel model)
        {
            _taxSettings = model.ToEntity(_taxSettings);

            var defaultAddress = _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId) ??
                                         new Core.Domain.Common.Address()
                                         {
                                             CreatedOnUtc = DateTime.UtcNow,
                                         };
            defaultAddress = model.DefaultTaxAddress.ToEntity(defaultAddress);
            if (defaultAddress.Id > 0)
                _addressService.UpdateAddress(defaultAddress);
            else
                _addressService.InsertAddress(defaultAddress);

            _taxSettings.DefaultTaxAddressId = defaultAddress.Id;
            _settingService.SaveSetting(_taxSettings);


            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }




        public ActionResult Catalog()
        {
            var model = _catalogSettings.ToModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult Catalog(CatalogSettingsModel model)
        {
            _catalogSettings = model.ToEntity(_catalogSettings);
            _settingService.SaveSetting(_catalogSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Catalog");
        }



        public ActionResult RewardPoints()
        {
            var model = _rewardPointsSettings.ToModel();
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            return View(model);
        }
        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model)
        {
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            if (ModelState.IsValid)
            {
                _rewardPointsSettings = model.ToEntity(_rewardPointsSettings);
                _settingService.SaveSetting(_rewardPointsSettings);
            }

            //If we got this far, something failed, redisplay form

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"), false);
            return View(model);
        }




        public ActionResult Order()
        {
            var model = _orderSettings.ToModel();
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            //parse return request actions
            for (int i = 0; i < _orderSettings.ReturnRequestActions.Count; i++)
            {
                model.ReturnRequestActionsParsed += _orderSettings.ReturnRequestActions[i];
                if (i != _orderSettings.ReturnRequestActions.Count - 1)
                    model.ReturnRequestActionsParsed += ",";
            }
            //parse return request reasons
            for (int i = 0; i < _orderSettings.ReturnRequestReasons.Count; i++)
            {
                model.ReturnRequestReasonsParsed += _orderSettings.ReturnRequestReasons[i];
                if (i != _orderSettings.ReturnRequestReasons.Count - 1)
                    model.ReturnRequestReasonsParsed += ",";
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Order(OrderSettingsModel model)
        {
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            _orderSettings = model.ToEntity(_orderSettings);

            //parse return request actions
            _orderSettings.ReturnRequestActions.Clear();
            foreach (var returnAction in model.ReturnRequestActionsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                _orderSettings.ReturnRequestActions.Add(returnAction);
            //parse return request reasons
            _orderSettings.ReturnRequestReasons.Clear();
            foreach (var returnReason in model.ReturnRequestReasonsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                _orderSettings.ReturnRequestReasons.Add(returnReason);

            _settingService.SaveSetting(_orderSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Order");
        }




        public ActionResult ShoppingCart()
        {
            var model = _shoppingCartSettings.ToModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult ShoppingCart(ShoppingCartSettingsModel model)
        {
            _shoppingCartSettings = model.ToEntity(_shoppingCartSettings);
            _settingService.SaveSetting(_shoppingCartSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }




        public ActionResult Media()
        {
            var model = _mediaSettings.ToModel();
            model.PicturesStoredIntoDatabase = _pictureService.StoreInDb;
            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Media(MediaSettingsModel model)
        {
            _mediaSettings = model.ToEntity(_mediaSettings);
            _settingService.SaveSetting(_mediaSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }
        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public ActionResult ChangePictureStorage()
        {
            _pictureService.StoreInDb = !_pictureService.StoreInDb;

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }



        public ActionResult CustomerUser()
        {
            //merge settings
            var model = new CustomerUserSettingsModel();
            model.CustomerSettings = _customerSettings.ToModel();
            model.UserSettings = _userSettings.ToModel();

            model.DateTimeSettings.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;
            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem()
                    {
                        Text = timeZone.DisplayName,
                        Value = timeZone.Id,
                        Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.InvariantCultureIgnoreCase)
                    });
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult CustomerUser(CustomerUserSettingsModel model)
        {
            _customerSettings = model.CustomerSettings.ToEntity(_customerSettings);
            _settingService.SaveSetting(_customerSettings);


            _userSettings = model.UserSettings.ToEntity(_userSettings);
            _settingService.SaveSetting(_userSettings);

            _dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            _dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            _settingService.SaveSetting(_dateTimeSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("CustomerUser");
        }






        public ActionResult GeneralCommon()
        {
            //store information
            var model = new GeneralCommonSettingsModel();
            model.StoreInformationSettings.StoreName = _storeInformationSettings.StoreName;
            model.StoreInformationSettings.StoreUrl = _storeInformationSettings.StoreUrl;

            //seo settings
            model.SeoSettings.PageTitleSeparator = _seoSettings.PageTitleSeparator;
            model.SeoSettings.DefaultTitle = _seoSettings.DefaultTitle;
            model.SeoSettings.DefaultMetaKeywords = _seoSettings.DefaultMetaKeywords;
            model.SeoSettings.DefaultMetaDescription = _seoSettings.DefaultMetaDescription;
            model.SeoSettings.ConvertNonWesternChars = _seoSettings.ConvertNonWesternChars;
            
            //security settings
            model.SecuritySettings.EncryptionKey = _securitySettings.EncryptionKey;
            if (_securitySettings.AdminAreaAllowedIpAddresses!=null)
                for (int i=0;i<_securitySettings.AdminAreaAllowedIpAddresses.Count; i++)
                {
                    model.SecuritySettings.AdminAreaAllowedIpAddresses += _securitySettings.AdminAreaAllowedIpAddresses[i];
                    if (i != _securitySettings.AdminAreaAllowedIpAddresses.Count - 1)
                        model.SecuritySettings.AdminAreaAllowedIpAddresses += ",";
                }
            

            //PDF settings
            model.PdfSettings.Enabled = _pdfSettings.Enabled;

            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult GeneralCommon(GeneralCommonSettingsModel model)
        {
            //store information
            _storeInformationSettings.StoreName = model.StoreInformationSettings.StoreName;
            if (model.StoreInformationSettings.StoreUrl == null)
                model.StoreInformationSettings.StoreUrl = "";
            _storeInformationSettings.StoreUrl = model.StoreInformationSettings.StoreUrl;
            //ensure we have "/" at the end
            if (!_storeInformationSettings.StoreUrl.EndsWith("/"))
                _storeInformationSettings.StoreUrl += "/";
            _settingService.SaveSetting(_storeInformationSettings);

            //seo settings
            _seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
            _seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
            _seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
            _seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
            _seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
            _settingService.SaveSetting(_seoSettings);


            //security settings
            if (_securitySettings.AdminAreaAllowedIpAddresses == null)
                _securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
            _securitySettings.AdminAreaAllowedIpAddresses.Clear();
            if (!String.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                foreach (string s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    if (!String.IsNullOrWhiteSpace(s))
                        _securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
            _settingService.SaveSetting(_securitySettings);

            //PDF settings
            _pdfSettings.Enabled = model.PdfSettings.Enabled;
            _settingService.SaveSetting(_pdfSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("GeneralCommon");
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new NopException("Encryption private key must be 16 characters long");

                string oldEncryptionPrivateKey = _securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new NopException("The new ecryption key is the same as the old one");

                //update encrypted order info
                var orders = _orderService.LoadAllOrders();
                foreach (var order in orders)
                {
                    string decryptedCardType = _encryptionService.DecryptText(order.CardType, oldEncryptionPrivateKey);
                    string decryptedCardName = _encryptionService.DecryptText(order.CardName, oldEncryptionPrivateKey);
                    string decryptedCardNumber = _encryptionService.DecryptText(order.CardNumber, oldEncryptionPrivateKey);
                    string decryptedMaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber, oldEncryptionPrivateKey);
                    string decryptedCardCvv2 = _encryptionService.DecryptText(order.CardCvv2, oldEncryptionPrivateKey);
                    string decryptedCardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth, oldEncryptionPrivateKey);
                    string decryptedCardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear, oldEncryptionPrivateKey);

                    string encryptedCardType = _encryptionService.EncryptText(decryptedCardType, newEncryptionPrivateKey);
                    string encryptedCardName = _encryptionService.EncryptText(decryptedCardName, newEncryptionPrivateKey);
                    string encryptedCardNumber = _encryptionService.EncryptText(decryptedCardNumber, newEncryptionPrivateKey);
                    string encryptedMaskedCreditCardNumber = _encryptionService.EncryptText(decryptedMaskedCreditCardNumber, newEncryptionPrivateKey);
                    string encryptedCardCvv2 = _encryptionService.EncryptText(decryptedCardCvv2, newEncryptionPrivateKey);
                    string encryptedCardExpirationMonth = _encryptionService.EncryptText(decryptedCardExpirationMonth, newEncryptionPrivateKey);
                    string encryptedCardExpirationYear = _encryptionService.EncryptText(decryptedCardExpirationYear, newEncryptionPrivateKey);

                    order.CardType = encryptedCardType;
                    order.CardName = encryptedCardName;
                    order.CardNumber = encryptedCardNumber;
                    order.MaskedCreditCardNumber = encryptedMaskedCreditCardNumber;
                    order.CardCvv2 = encryptedCardCvv2;
                    order.CardExpirationMonth = encryptedCardExpirationMonth;
                    order.CardExpirationYear = encryptedCardExpirationYear;
                    _orderService.UpdateOrder(order);
                }

                //update user information
                //TODO optimization - load only users with PasswordFormat.Encrypted (don't filter them here)
                var users = _userService.GetUsers(null, null, 0, int.MaxValue)
                    .Where(u => u.PasswordFormat == PasswordFormat.Encrypted);
                foreach (var user in users)
                {
                    string decryptedPassword = _encryptionService.DecryptText(user.Password, oldEncryptionPrivateKey);
                    string encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    user.Password = encryptedPassword;
                    _userService.UpdateUser(user);
                }

                _securitySettings.EncryptionKey = newEncryptionPrivateKey;
                _settingService.SaveSetting(_securitySettings);
                SuccessNotification("Encryption key is changed");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }
            return RedirectToAction("GeneralCommon");
        }




        //all settings
        public ActionResult AllSettings()
        {
            var settings = _settingService.GetAllSettings().Select(x => x.Value).OrderBy(x => x.Name).ToList();
            var model = new GridModel<SettingModel>
            {
                Data = settings.Take(20).Select(x => 
                {
                    return new SettingModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value
                    };
                }),
                Total = settings.Count
            };
            return View(model);
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult AllSettings(GridCommand command)
        {
            var settings = _settingService.GetAllSettings().Select(x => x.Value).OrderBy(x => x.Name)
                .Select(x => 
                {
                    return new SettingModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value
                    };
                })
                .ForCommand(command);

            var model = new GridModel<SettingModel>
            {
                Data = settings.PagedForCommand(command),
                Total = settings.Count()
            };
            return new JsonResult
            {
                Data = model
            };
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingUpdate(SettingModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AllSettings");
            }

            var setting = _settingService.GetSettingById(model.Id);
            if (setting.Name != model.Name)
                _settingService.DeleteSetting(setting);

            _settingService.SetSetting(model.Name, model.Value);

            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingAdd([Bind(Exclude = "Id")] SettingModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult { Data = "error" };
            }

            _settingService.SetSetting(model.Name, model.Value);
            
            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingDelete(int id, GridCommand command)
        {
            var setting = _settingService.GetSettingById(id);
            _settingService.DeleteSetting(setting);
            
            return AllSettings(command);
        }

        #endregion
    }
}
