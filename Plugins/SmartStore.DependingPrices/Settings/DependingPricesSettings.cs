using SmartStore.Core.Configuration;

namespace SmartStore.DependingPrices.Settings
{
    public class DependingPricesSettings : ISettings
    {
        public bool ShowPriceAsDiscount { get; set; }

        public string CustomerRolesForProductCost { get; set; }

		public bool HideTierPrices { get; set; }

		public bool ApplyTierPriceAsBase { get; set; }

		public string CustomerRolesForPriceSupression { get; set; }

        public bool ProcessVariantCombinations { get; set; } = true;
    }
}