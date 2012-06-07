using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Localization;

namespace Nop.Plugin.Widgets.LivePersonChat
{
    /// <summary>
    /// Live person provider
    /// </summary>
    public class LivePersonChatPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly LivePersonChatSettings _livePersonChatSettings;

        public LivePersonChatPlugin(LivePersonChatSettings livePersonChatSettings)
        {
            this._livePersonChatSettings = livePersonChatSettings;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return !string.IsNullOrWhiteSpace(_livePersonChatSettings.WidgetZone)
                       ? new List<string>() { _livePersonChatSettings.WidgetZone }
                       : new List<string>() { "before_left_side_column" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsLivePersonChat";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Widgets.LivePersonChat.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsLivePersonChat";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Widgets.LivePersonChat.Controllers" }, { "area", null }, };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.LivePersonChat.ButtonCode", "Button code(max 2000)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.LivePersonChat.ButtonCode.Hint", "Enter your button code here.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.LivePersonChat.LiveChat", "Live chat");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.LivePersonChat.MonitoringCode", "Monitoring code(max 2000)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.LivePersonChat.MonitoringCode.Hint", "Enter your monitoring code here.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Widgets.LivePersonChat.ButtonCode");
            this.DeletePluginLocaleResource("Plugins.Widgets.LivePersonChat.ButtonCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.LivePersonChat.LiveChat");
            this.DeletePluginLocaleResource("Plugins.Widgets.LivePersonChat.MonitoringCode");
            this.DeletePluginLocaleResource("Plugins.Widgets.LivePersonChat.MonitoringCode.Hint");

            base.Uninstall();
        }
    }
}
