using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.DependingPrices.Domain;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DependingPrices.Models
{
    //[CustomModelPart]
    public class DependingPricesTabModel : ModelBase
    {
        public DependingPricesTabModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableLanguages = new List<SelectListItem>();
        }

        public int ProductId { get; set; }
		public bool IsPluginConfig { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerNumber")]
        public string CustomerNumber { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerGroupId")]
        public int CustomerGroupId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.LanguageId")]
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.StoreId")]
        public int StoreId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.Quantity")]
        public int? Quantity { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.Price")]
        public decimal Price { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CalculationMethod")]
		public DependingPriceCalculationMethod CalculationMethod { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.ItemGroupId")]
		public int ItemGroupId { get; set; }

		public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }
    }

    //[CustomModelPart]
    public class FilterCriteria : EntityModelBase
	{
		public int? CustomerGroupId { get; set; }
		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerGroupId")]
		[UIHint("DependingPriceCustomer")]
		public string CustomerGroup { get; set; }

		public int? LanguageId { get; set; }
		[UIHint("DependingPriceLanguage")]
		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.LanguageId")]
		public string Language { get; set; }

		public int? StoreId { get; set; }
		[UIHint("DependingPriceStore")]
		[SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.StoreId")]
		public string Store { get; set; }

        [UIHint("DependingPriceFilterDecimal")]
        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.Price")]
		//we don't name it Price because Telerik has a small bug 
		//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//in our case it's product.Price
		public decimal FilterDecimal { get; set; }

        public int ProductId { get; set; }

        [UIHint("DependingPriceItemGroupId")]
        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.ItemGroupId")]
		public int ItemGroupId { get; set; }

        [UIHint("DependingPriceCustomerNumber")]
        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CustomerNumber")]
        public string CustomerNumber { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.Quantity")]
        [UIHint("DependingPriceQuantity")]
        public int? Quantity { get; set; }

        public int CalculationMethodId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.DependingPrices.CalculationMethod")]
		[UIHint("DependingPriceCalculationMethod")]
		public string CalculationMethod { get; set; }
	}

    public class GridEditOption
    {
        public int id { get; set; }
        public string name { get; set; }
        public string text { get; set; }
    }
}