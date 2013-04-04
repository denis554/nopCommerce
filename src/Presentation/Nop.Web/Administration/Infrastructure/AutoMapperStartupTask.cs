﻿using AutoMapper;
using Nop.Admin.Models.Blogs;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Cms;
using Nop.Admin.Models.Common;
using Nop.Admin.Models.Customers;
using Nop.Admin.Models.Directory;
using Nop.Admin.Models.Discounts;
using Nop.Admin.Models.ExternalAuthentication;
using Nop.Admin.Models.Forums;
using Nop.Admin.Models.Localization;
using Nop.Admin.Models.Logging;
using Nop.Admin.Models.Messages;
using Nop.Admin.Models.News;
using Nop.Admin.Models.Orders;
using Nop.Admin.Models.Payments;
using Nop.Admin.Models.Plugins;
using Nop.Admin.Models.Polls;
using Nop.Admin.Models.Settings;
using Nop.Admin.Models.Shipping;
using Nop.Admin.Models.Tax;
using Nop.Admin.Models.Topics;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Core.Domain.Stores;
using Nop.Admin.Models.Stores;

namespace Nop.Admin.Infrastructure
{
    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            //TODO remove 'CreatedOnUtc' ignore mappings because now presentation layer models have 'CreatedOn' property and core entities have 'CreatedOnUtc' property (distinct names)
            
            //address
            Mapper.CreateMap<Address, AddressModel>()
                .ForMember(dest => dest.AddressHtml, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCountries, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStates, mo => mo.Ignore())
                .ForMember(dest => dest.FirstNameEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.FirstNameRequired, mo => mo.Ignore())
                .ForMember(dest => dest.LastNameEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.LastNameRequired, mo => mo.Ignore())
                .ForMember(dest => dest.EmailEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.EmailRequired, mo => mo.Ignore())
                .ForMember(dest => dest.CompanyEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CompanyRequired, mo => mo.Ignore())
                .ForMember(dest => dest.CountryEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.StateProvinceEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CityEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.CityRequired, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddressEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddressRequired, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddress2Enabled, mo => mo.Ignore())
                .ForMember(dest => dest.StreetAddress2Required, mo => mo.Ignore())
                .ForMember(dest => dest.ZipPostalCodeEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.ZipPostalCodeRequired, mo => mo.Ignore())
                .ForMember(dest => dest.PhoneEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.PhoneRequired, mo => mo.Ignore())
                .ForMember(dest => dest.FaxEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.FaxRequired, mo => mo.Ignore())
                .ForMember(dest => dest.CountryName, mo => mo.MapFrom(src => src.Country != null ? src.Country.Name : null))
                .ForMember(dest => dest.StateProvinceName, mo => mo.MapFrom(src => src.StateProvince != null ? src.StateProvince.Name : null))
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<AddressModel, Address>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Country, mo => mo.Ignore())
                .ForMember(dest => dest.StateProvince, mo => mo.Ignore());

            //countries
            Mapper.CreateMap<CountryModel, Country>()
                .ForMember(dest => dest.StateProvinces, mo => mo.Ignore())
                .ForMember(dest => dest.RestrictedShippingMethods, mo => mo.Ignore());
            Mapper.CreateMap<Country, CountryModel>()
                .ForMember(dest => dest.NumberOfStates, mo => mo.MapFrom(src => src.StateProvinces != null ? src.StateProvinces.Count : 0))
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //state/provinces
            Mapper.CreateMap<StateProvince, StateProvinceModel>()
                .ForMember(dest => dest.DisplayOrder1, mo => mo.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<StateProvinceModel, StateProvince>()
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.DisplayOrder1))
                .ForMember(dest => dest.Country, mo => mo.Ignore());

            //language
            Mapper.CreateMap<Language, LanguageModel>()
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.FlagFileNames, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<LanguageModel, Language>()
                .ForMember(dest => dest.LocaleStringResources, mo => mo.Ignore());
            //email account
            Mapper.CreateMap<EmailAccount, EmailAccountModel>()
                .ForMember(dest => dest.Password, mo => mo.Ignore()) 
                .ForMember(dest => dest.IsDefaultEmailAccount, mo => mo.Ignore()) 
                .ForMember(dest => dest.SendTestEmailTo, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<EmailAccountModel, EmailAccount>()
                .ForMember(dest => dest.Password, mo => mo.Ignore());
            //message template
            Mapper.CreateMap<MessageTemplate, MessageTemplateModel>()
                .ForMember(dest => dest.AllowedTokens, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<MessageTemplateModel, MessageTemplate>();
            //queued email
            Mapper.CreateMap<QueuedEmail, QueuedEmailModel>()
                .ForMember(dest => dest.EmailAccountName, mo => mo.MapFrom(src => src.EmailAccount != null ? src.EmailAccount.FriendlyName : string.Empty))
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.SentOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<QueuedEmailModel, QueuedEmail>()
                .ForMember(dest=> dest.CreatedOnUtc, dt=> dt.Ignore())
                .ForMember(dest => dest.SentOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EmailAccount, mo => mo.Ignore())
                .ForMember(dest => dest.EmailAccountId, mo => mo.Ignore());
            //campaign
            Mapper.CreateMap<Campaign, CampaignModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.AllowedTokens, mo => mo.Ignore())
                .ForMember(dest => dest.TestEmail, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CampaignModel, Campaign>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //topcis
            Mapper.CreateMap<Topic, TopicModel>()
                .ForMember(dest => dest.Url, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<TopicModel, Topic>();

            //category
            Mapper.CreateMap<Category, CategoryModel>()
                .ForMember(dest => dest.AvailableCategoryTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.Breadcrumb, mo => mo.Ignore())
                .ForMember(dest => dest.ParentCategories, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CategoryModel, Category>()
                .ForMember(dest => dest.HasDiscountsApplied, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore());
            //manufacturer
            Mapper.CreateMap<Manufacturer, ManufacturerModel>()
                .ForMember(dest => dest.AvailableManufacturerTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ManufacturerModel, Manufacturer>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore());
            //products
            Mapper.CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                .ForMember(dest => dest.PictureThumbnailUrl, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableVendors, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableProductTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariantModels, mo => mo.Ignore())
                .ForMember(dest => dest.FirstProductVariantModel, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfAvailableCategories, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfAvailableManufacturers, mo => mo.Ignore())
                .ForMember(dest => dest.AddPictureModel, mo => mo.Ignore())
                .ForMember(dest => dest.ProductPictureModels, mo => mo.Ignore())
                .ForMember(dest => dest.AddSpecificationAttributeModel, mo => mo.Ignore())
                .ForMember(dest => dest.CopyProductModel, mo => mo.Ignore())
                .ForMember(dest => dest.IsLoggedInAsVendor, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ProductModel, Product>()
                .ForMember(dest => dest.ProductTags, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedRatingSum, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedRatingSum, mo => mo.Ignore())
                .ForMember(dest => dest.ApprovedTotalReviews, mo => mo.Ignore())
                .ForMember(dest => dest.NotApprovedTotalReviews, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariants, mo => mo.Ignore())
                .ForMember(dest => dest.ProductCategories, mo => mo.Ignore())
                .ForMember(dest => dest.ProductManufacturers, mo => mo.Ignore())
                .ForMember(dest => dest.ProductPictures, mo => mo.Ignore())
                .ForMember(dest => dest.ProductReviews, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore());
            //product variants
            Mapper.CreateMap<ProductVariant, ProductVariantModel>()
                .ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.BaseDimensionIn, mo => mo.Ignore())
                .ForMember(dest => dest.BaseWeightIn, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfAvailableProductAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                .ForMember(dest => dest.HideNameAndDescriptionProperties, mo => mo.Ignore())
                .ForMember(dest => dest.HidePublishedProperty, mo => mo.Ignore())
                .ForMember(dest => dest.HideDisplayOrderProperty, mo => mo.Ignore())
                .ForMember(dest => dest.CopyProductVariantModel, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ProductVariantModel, ProductVariant>()
                .ForMember(dest => dest.HasTierPrices, mo => mo.Ignore())
                .ForMember(dest => dest.HasDiscountsApplied, mo => mo.Ignore())
                .ForMember(dest => dest.BackorderMode, mo => mo.Ignore())
                .ForMember(dest => dest.DownloadActivationType, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                .ForMember(dest => dest.LowStockActivity, mo => mo.Ignore())
                .ForMember(dest => dest.ManageInventoryMethod, mo => mo.Ignore())
                .ForMember(dest => dest.RecurringCyclePeriod, mo => mo.Ignore())
                .ForMember(dest => dest.Product, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariantAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariantAttributeCombinations, mo => mo.Ignore())
                .ForMember(dest => dest.TierPrices, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore());
            //logs
            Mapper.CreateMap<Log, LogModel>()
                .ForMember(dest => dest.CustomerEmail, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<LogModel, Log>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.LogLevelId, mo => mo.Ignore())
                .ForMember(dest => dest.Customer, mo => mo.Ignore());
            //ActivityLogType
            Mapper.CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(dest => dest.SystemKeyword, mo => mo.Ignore());
            Mapper.CreateMap<ActivityLogType, ActivityLogTypeModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(dest => dest.ActivityLogTypeName, mo => mo.MapFrom(src => src.ActivityLogType.Name))
                .ForMember(dest => dest.CustomerEmail, mo => mo.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //currencies
            Mapper.CreateMap<Currency, CurrencyModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.IsPrimaryExchangeRateCurrency, mo => mo.Ignore())
                .ForMember(dest => dest.IsPrimaryStoreCurrency, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CurrencyModel, Currency>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
            //measure weights
            Mapper.CreateMap<MeasureWeight, MeasureWeightModel>()
                .ForMember(dest => dest.IsPrimaryWeight, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<MeasureWeightModel, MeasureWeight>();
            //measure dimensions
            Mapper.CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(dest => dest.IsPrimaryDimension, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<MeasureDimensionModel, MeasureDimension>();
            //tax providers
            Mapper.CreateMap<ITaxProvider, TaxProviderModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.IsPrimaryTaxProvider, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //tax categories
            Mapper.CreateMap<TaxCategory, TaxCategoryModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<TaxCategoryModel, TaxCategory>();
            //shipping methods
            Mapper.CreateMap<ShippingMethod, ShippingMethodModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ShippingMethodModel, ShippingMethod>()
                .ForMember(dest => dest.RestrictedCountries, mo => mo.Ignore());
            //shipping rate computation methods
            Mapper.CreateMap<IShippingRateComputationMethod, ShippingRateComputationMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //payment methods
            Mapper.CreateMap<IPaymentMethod, PaymentMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.RecurringPaymentType, mo => mo.MapFrom(src => src.RecurringPaymentType.ToString()))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //external authentication methods
            Mapper.CreateMap<IExternalAuthenticationMethod, AuthenticationMethodModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //widgets
            Mapper.CreateMap<IWidgetPlugin, WidgetModel>()
                .ForMember(dest => dest.FriendlyName, mo => mo.MapFrom(src => src.PluginDescriptor.FriendlyName))
                .ForMember(dest => dest.SystemName, mo => mo.MapFrom(src => src.PluginDescriptor.SystemName))
                .ForMember(dest => dest.DisplayOrder, mo => mo.MapFrom(src => src.PluginDescriptor.DisplayOrder))
                .ForMember(dest => dest.IsActive, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationActionName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationControllerName, mo => mo.Ignore())
                .ForMember(dest => dest.ConfigurationRouteValues, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //plugins
            Mapper.CreateMap<PluginDescriptor, PluginModel>()
                .ForMember(dest => dest.ConfigurationUrl, mo => mo.Ignore())
                .ForMember(dest => dest.CanChangeEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.IsEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            //newsLetter subscriptions
            Mapper.CreateMap<NewsLetterSubscription, NewsLetterSubscriptionModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<NewsLetterSubscriptionModel, NewsLetterSubscription>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.NewsLetterSubscriptionGuid, mo => mo.Ignore());
            //forums
            Mapper.CreateMap<ForumGroup, ForumGroupModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ForumModels, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ForumGroupModel, ForumGroup>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Forums, mo => mo.Ignore());
            Mapper.CreateMap<Forum, ForumModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ForumGroups, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ForumModel, Forum>()
                .ForMember(dest => dest.NumTopics, mo => mo.Ignore())
                .ForMember(dest => dest.NumPosts, mo => mo.Ignore())
                .ForMember(dest => dest.LastTopicId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostCustomerId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostTime, mo => mo.Ignore())
                .ForMember(dest => dest.ForumGroup, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
            //blogs
            Mapper.CreateMap<BlogPost, BlogPostModel>()
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(src.LanguageId, true, false)))
                .ForMember(dest => dest.Comments, mo => mo.Ignore())
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<BlogPostModel, BlogPost>()
                .ForMember(dest => dest.BlogComments, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.CommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //news
            Mapper.CreateMap<NewsItem, NewsItemModel>()
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(src.LanguageId, true, false)))
                .ForMember(dest => dest.Comments, mo => mo.Ignore())
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<NewsItemModel, NewsItem>()
                .ForMember(dest => dest.NewsComments, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.CommentCount, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //news
            Mapper.CreateMap<Poll, PollModel>()
                .ForMember(dest => dest.StartDate, mo => mo.Ignore())
                .ForMember(dest => dest.EndDate, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<PollModel, Poll>()
                .ForMember(dest => dest.PollAnswers, mo => mo.Ignore())
                .ForMember(dest => dest.Language, mo => mo.Ignore())
                .ForMember(dest => dest.StartDateUtc, mo => mo.Ignore())
                .ForMember(dest => dest.EndDateUtc, mo => mo.Ignore());
            //customer roles
            Mapper.CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CustomerRoleModel, CustomerRole>()
                .ForMember(dest => dest.PermissionRecords, mo => mo.Ignore());

            //product attributes
            Mapper.CreateMap<ProductAttribute, ProductAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ProductAttributeModel, ProductAttribute>();
            //specification attributes
            Mapper.CreateMap<SpecificationAttribute, SpecificationAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeModel, SpecificationAttribute>()
                .ForMember(dest => dest.SpecificationAttributeOptions, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<SpecificationAttributeOptionModel, SpecificationAttributeOption>()
                .ForMember(dest => dest.SpecificationAttribute, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSpecificationAttributes, mo => mo.Ignore());
            //checkout attributes
            Mapper.CreateMap<CheckoutAttribute, CheckoutAttributeModel>()
                .ForMember(dest => dest.AvailableTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.AttributeControlTypeName, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CheckoutAttributeModel, CheckoutAttribute>()
                .ForMember(dest => dest.AttributeControlType, mo => mo.Ignore())
                .ForMember(dest => dest.CheckoutAttributeValues, mo => mo.Ignore());
            //discounts
            Mapper.CreateMap<Discount, DiscountModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.AddDiscountRequirement, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscountRequirementRules, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountRequirementMetaInfos, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToCategoryModels, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToProductVariantModels, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<DiscountModel, Discount>()
                .ForMember(dest => dest.DiscountType, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountLimitation, mo => mo.Ignore())
                .ForMember(dest => dest.DiscountRequirements, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToCategories, mo => mo.Ignore())
                .ForMember(dest => dest.AppliedToProductVariants, mo => mo.Ignore());
            //gift cards
            Mapper.CreateMap<GiftCard, GiftCardModel>()
                .ForMember(dest => dest.PurchasedWithOrderId, mo => mo.Ignore())
                .ForMember(dest => dest.AmountStr, mo => mo.Ignore())
                .ForMember(dest => dest.RemainingAmountStr, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<GiftCardModel, GiftCard>()
                .ForMember(dest => dest.PurchasedWithOrderProductVariantId, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardType, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCardUsageHistory, mo => mo.Ignore())
                .ForMember(dest => dest.PurchasedWithOrderProductVariant, mo => mo.Ignore())
                .ForMember(dest => dest.IsRecipientNotified, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore());
            //stores
            Mapper.CreateMap<Store, StoreModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<StoreModel, Store>();

            //Settings
            Mapper.CreateMap<TaxSettings, TaxSettingsModel>()
                .ForMember(dest => dest.DefaultTaxAddress, mo => mo.Ignore())
                .ForMember(dest => dest.TaxDisplayTypeValues, mo => mo.Ignore())
                .ForMember(dest => dest.TaxBasedOnValues, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingTaxCategories, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatShopCountries, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.PricesIncludeTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToSelectTaxDisplayType_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TaxDisplayType_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTaxSuffix_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTaxRates_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.HideZeroTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.HideTaxInOrderSummary_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TaxBasedOn_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultTaxAddress_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingIsTaxable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingPriceIncludesTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingTaxClassId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatShopCountryId_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatAllowVatExemption_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatUseWebService_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<TaxSettingsModel, TaxSettings>()
                .ForMember(dest => dest.ActiveTaxProviderSystemName, mo => mo.Ignore());
            Mapper.CreateMap<NewsSettings, NewsSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyAboutNewNewsComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowNewsOnMainPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MainPageNewsCount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NewsArchivePageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<NewsSettingsModel, NewsSettings>();
            Mapper.CreateMap<ForumSettings, ForumSettingsModel>()
                .ForMember(dest => dest.ForumEditorValues, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.ForumsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.RelativeDateTimeFormattingEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowCustomersPostCount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowGuestsToCreatePosts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowGuestsToCreateTopics_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToEditPosts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToDeletePosts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCustomersToManageSubscriptions_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TopicsPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PostsPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ForumEditor_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.SignaturesEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowPrivateMessages_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowAlertForPM_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyAboutPrivateMessages_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveDiscussionsFeedEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveDiscussionsFeedCount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ForumFeedsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ForumFeedCount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.SearchResultsPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ForumSettingsModel, ForumSettings>()
                .ForMember(dest => dest.TopicSubjectMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.StrippedTopicMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.PostMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.TopicPostsPageLinkDisplayCount, mo => mo.Ignore())
                .ForMember(dest => dest.LatestCustomerPostsPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.PrivateMessagesPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.ForumSubscriptionsPageSize, mo => mo.Ignore())
                .ForMember(dest => dest.PMSubjectMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.PMTextMaxLength, mo => mo.Ignore())
                .ForMember(dest => dest.HomePageActiveDiscussionsTopicCount, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveDiscussionsPageTopicCount, mo => mo.Ignore())
                .ForMember(dest => dest.ForumSearchTermMinimumLength, mo => mo.Ignore());
            Mapper.CreateMap<BlogSettings, BlogSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PostsPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyAboutNewBlogComments_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfTags_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowHeaderRssUrl_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<BlogSettingsModel, BlogSettings>();
            Mapper.CreateMap<ShippingSettings, ShippingSettingsModel>()
                .ForMember(dest => dest.ShippingOriginAddress, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXValue_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.FreeShippingOverXIncludingTax_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EstimateShippingEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayShipmentEventsToCustomers_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShippingOriginAddress_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ShippingSettingsModel, ShippingSettings>()
                .ForMember(dest => dest.ActiveShippingRateComputationMethodSystemNames, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnValidOptionsIfThereAreAny, mo => mo.Ignore());
            Mapper.CreateMap<CatalogSettings, CatalogSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductSku_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowManufacturerPartNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowGtin_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowProductSorting_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowProductViewModeChanging_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductsFromSubcategories_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowCategoryProductNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CategoryBreadcrumbEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowShareButton_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductReviewsMustBeApproved_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowAnonymousUsersToReviewProduct_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EmailAFriendEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowAnonymousUsersToEmailAFriend_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.RecentlyViewedProductsNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.RecentlyViewedProductsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.RecentlyAddedProductsNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.RecentlyAddedProductsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CompareProductsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowBestsellersOnHomepage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfBestsellersOnHomepage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.SearchPageProductsPerPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSearchAutoCompleteEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesInSearchAutoComplete_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductsAlsoPurchasedEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductsAlsoPurchasedNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EnableDynamicPriceUpdate_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfProductTags_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductsByTagPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductsByTagPageSizeOptions_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.IncludeShortDescriptionInCompareProducts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.IncludeFullDescriptionInCompareProducts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.IgnoreDiscounts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.IgnoreFeaturedProducts_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ManufacturersBlockItemsToDisplay_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CatalogSettingsModel, CatalogSettings>()
                .ForMember(dest => dest.DefaultViewMode, mo => mo.Ignore())
                .ForMember(dest => dest.PageShareCode, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultProductRatingValue, mo => mo.Ignore())
                .ForMember(dest => dest.ProductSearchTermMinimumLength, mo => mo.Ignore())
                .ForMember(dest => dest.IncludeFeaturedProductsInNormalLists, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultCategoryPageSizeOptions, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultManufacturerPageSizeOptions, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumBackInStockSubscriptions, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayTierPricesWithDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.CompareProductsNumber, mo => mo.Ignore())
                .ForMember(dest => dest.FileUploadMaximumSizeBytes, mo => mo.Ignore())
                .ForMember(dest => dest.FileUploadAllowedExtensions, mo => mo.Ignore());
            Mapper.CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.Enabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ExchangeRate_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinimumRewardPointsToUse_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForRegistration_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_Awarded_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.PointsForPurchases_Canceled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();
            Mapper.CreateMap<OrderSettings, OrderSettingsModel>()
                .ForMember(dest => dest.ReturnRequestReasonsParsed, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnRequestActionsParsed, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCards_Activated_OrderStatuses, mo => mo.Ignore())
                .ForMember(dest => dest.GiftCards_Deactivated_OrderStatuses, mo => mo.Ignore())
                .ForMember(dest => dest.PrimaryStoreCurrencyCode, mo => mo.Ignore())
                .ForMember(dest => dest.OrderIdent, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.IsReOrderAllowed_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinOrderSubtotalAmount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MinOrderTotalAmount_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AnonymousCheckoutAllowed_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.TermsOfServiceEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.OnePageCheckoutEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnRequestsEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.NumberOfDaysReturnRequestAvailable_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<OrderSettingsModel, OrderSettings>()
                .ForMember(dest => dest.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnRequestReasons, mo => mo.Ignore())
                .ForMember(dest => dest.ReturnRequestActions, mo => mo.Ignore())
                .ForMember(dest => dest.MinimumOrderPlacementInterval, mo => mo.Ignore());
            Mapper.CreateMap<ShoppingCartSettings, ShoppingCartSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayCartAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayWishlistAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumShoppingCartItems_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumWishlistItems_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MoveItemsFromWishlistToCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesOnShoppingCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesOnWishList_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowDiscountBox_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowGiftCardBox_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CrossSellsNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EmailWishlistEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowAnonymousUsersToEmailWishlist_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MiniShoppingCartEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesInMiniShoppingCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MiniShoppingCartProductNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<ShoppingCartSettingsModel, ShoppingCartSettings>()
                .ForMember(dest => dest.RoundPricesDuringCalculation, mo => mo.Ignore());
            Mapper.CreateMap<MediaSettings, MediaSettingsModel>()
                .ForMember(dest => dest.PicturesStoredIntoDatabase, mo => mo.Ignore())
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.AvatarPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductDetailsPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ProductVariantPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CategoryThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ManufacturerThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CartThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MiniCartThumbPictureSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumImageSize_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<MediaSettingsModel, MediaSettings>()
                .ForMember(dest => dest.DefaultPictureZoomEnabled, mo => mo.Ignore())
                .ForMember(dest => dest.DefaultImageQuality, mo => mo.Ignore())
                .ForMember(dest => dest.MultipleThumbDirectories, mo => mo.Ignore())
                .ForMember(dest => dest.AutoCompleteSearchThumbPictureSize, mo => mo.Ignore());
            Mapper.CreateMap<CustomerSettings, CustomerUserSettingsModel.CustomerSettingsModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CustomerUserSettingsModel.CustomerSettingsModel, CustomerSettings>()
                .ForMember(dest => dest.HashedPasswordFormat, mo => mo.Ignore())
                .ForMember(dest => dest.PasswordMinLength, mo => mo.Ignore())
                .ForMember(dest => dest.AvatarMaximumSizeBytes, mo => mo.Ignore())
                .ForMember(dest => dest.DownloadableProductsValidateUser, mo => mo.Ignore())
                .ForMember(dest => dest.OnlineCustomerMinutes, mo => mo.Ignore())
                .ForMember(dest => dest.SuffixDeletedCustomers, mo => mo.Ignore());
            Mapper.CreateMap<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>()
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>();
        }
        
        public int Order
        {
            get { return 0; }
        }
    }
}