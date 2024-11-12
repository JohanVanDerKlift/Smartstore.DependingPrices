using System;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;
using SmartStore.DependingPrices.Settings;
using System.Web.Routing;
using SmartStore.Services.Cms;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using SmartStore.DependingPrices.Data.Migrations;
using SmartStore.Licensing;
using SmartStore.Core.Utilities;

namespace SmartStore.DependingPrices
{
    [LicensableModule]
    public partial class Plugin : BasePlugin, IConfigurable, IWidget
	{
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
		private readonly DependingPricesSettings _dependingPricesSettings;
		
		public Plugin(
            ISettingService settingService,
            ILocalizationService localizationService,
			DependingPricesSettings dependingPricesSettings)
        {
            _settingService = settingService;
            _localizationService = localizationService;
			_dependingPricesSettings = dependingPricesSettings;
		}

		public static bool CheckLicense()
		{
			return Throttle.Check("DP+J+3@dhUgw+Z2kBJ+xSb#efXM#8Y#R", TimeSpan.FromHours(24), true, () => LicenseChecker.CheckState("SmartStore.DependingPrices") > LicensingState.Unlicensed);
		}

		public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            //defaults
            var dependingPricesSettings = new DependingPricesSettings();
            _settingService.SaveSetting(dependingPricesSettings);

            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.DependingPrices");

            _settingService.DeleteSetting<DependingPricesSettings>();

            var migrator = new DbMigrator(new Configuration());
            migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>() { "content_before", "productbox_add_info" };
        }

        public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "DependingPrices";

			if (widgetZone.Equals("productbox_add_info"))
			{
				actionName = "ListItemPublicInfo";
			}

            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "SmartStore.DependingPrices.Controllers"},
                {"area", "SmartStore.DependingPrices"},
                {"widgetZone", widgetZone},
                {"model", model}
            };
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
            controllerName = "DependingPrices";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.DependingPrices" } };
        }
    }
}
