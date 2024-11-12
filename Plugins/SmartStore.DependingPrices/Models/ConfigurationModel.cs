using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.DependingPrices.Models
{
	//[CustomModelPart]
	public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerRolesForProductCost")]
        public string[] CustomerRolesForProductCost { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerRolesForPriceSupression")]
		public string[] CustomerRolesForPriceSupression { get; set; }

		public MultiSelectList AvailableCustomerRoles { get; set; }
		public MultiSelectList AvailableCustomerRoles2 { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.HideTierPrices")]
		public bool HideTierPrices { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.ApplyTierPriceAsBase")]
		public bool ApplyTierPriceAsBase { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.ProcessVariantCombinations")]
		public bool ProcessVariantCombinations { get; set; }

		// TODO
		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.ShowPriceAsDiscount")]
		public bool ShowPriceAsDiscount { get; set; }
	}
}