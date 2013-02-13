using System;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;

namespace Nop.Plugin.DiscountRules.Store
{
    public partial class StoreDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;

        public StoreDiscountRequirementRule(ISettingService settingService)
        {
            this._settingService = settingService;
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

            if (request.Customer == null)
                return false;

            var storeId = _settingService.GetSettingByKey<int>(string.Format("DiscountRequirement.Store-{0}", request.DiscountRequirement.Id));

            if (storeId == 0)
                return false;

            bool result = request.Store.Id == storeId;
            return result;
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
            string result = "Plugins/DiscountRulesStore/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.SelectStore", "Select store");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store", "Store");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store.Hint", "Select the store in which this discount will be valid.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.SelectStore");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.Store.Fields.Store.Hint");
            base.Uninstall();
        }
    }
}