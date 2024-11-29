using System;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Discounts;
using SmartStore.Services.Media;
using SmartStore.Services.Tax;
using SmartStore.Services.Catalog;
using SmartStore.DependingPrices.Services;
using SmartStore.DependingPrices.Settings;
using SmartStore.Services.Configuration;
using System.Linq;
using SmartStore.Core.Domain.Tax;
using SmartStore.Licensing;
using SmartStore.DependingPrices.Domain;
using SmartStore.Services.Common;
using System.Collections.Generic;
using SmartStore.DependingPrices.Data.Migrations;
using System.Runtime.Remoting.Contexts;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class MyPriceCalculationService : PriceCalculationService
    {
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
		private readonly IProductAttributeService _productAttributeService;
		private readonly IDownloadService _downloadService;
		private readonly ICommonServices _services;
		private readonly HttpRequestBase _httpRequestBase;
		private readonly ITaxService _taxService;
		private readonly TaxSettings _taxSettings;
		private readonly IDependingPricesService _dependingPricesService;
        private readonly ISettingService _settingService;
		private readonly IGenericAttributeService _genericAttributeService;
		
		public MyPriceCalculationService(
            IDiscountService discountService,
			ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeParser productAttributeParser,
			IProductService productService,
			ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings,
			IProductAttributeService productAttributeService,
			IDownloadService downloadService,
			ICommonServices services,
			HttpRequestBase httpRequestBase,
			ITaxService taxService,
			TaxSettings taxSettings,
			IDependingPricesService dependingPricesService,
            ISettingService settingService,
			IGenericAttributeService genericAttributeService)
            : base(discountService, categoryService, manufacturerService, productAttributeParser, productService, catalogSettings, productAttributeService, downloadService, services, httpRequestBase, taxService, taxSettings)
        {
            _discountService = discountService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productAttributeParser = productAttributeParser;
			_productService = productService;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
			_productAttributeService = productAttributeService;
			_downloadService = downloadService;
			_services = services;
			_httpRequestBase = httpRequestBase;
			_taxService = taxService;
			_taxSettings = taxSettings;
			_dependingPricesService = dependingPricesService;
            _settingService = settingService;
			_genericAttributeService = genericAttributeService;
		}
        
        /// <summary>
        /// Gets the final price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
		/// <param name="bundleItem">A product bundle item</param>
        /// <returns>Final price</returns>
        /// 

        //public override decimal GetPercentage(
        //    Product product,
        //    Customer customer,
        //    decimal additionalCharge,
        //    bool includeDiscounts,
        //    int quantity,
        //    ProductBundleItemData bundleItem = null,
        //    PriceCalculationContext context = null,
        //    bool isTierPrice = false)
        //{
        //    if (!DependingPrices.Plugin.CheckLicense())
        //        return base.GetPercentage(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem, context, isTierPrice);

        //    var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);
        //    var isVariant = product.MergedDataValues != null && product.MergedDataValues.Count > 0;
        //    if (isVariant)
        //    {
        //        if (!dependingPricesSettings.ProcessVariantCombinations)
        //        {
        //            return base.GetPercentage(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem, context, isTierPrice);
        //        }
        //    }

        //    var currentCustomerRoles = _services.WorkContext.CurrentCustomer.CustomerRoleMappings.Select(x => x.CustomerRole).Where(cr => cr.Active);
        //    var itemGroupId = _genericAttributeService.GetAttribute<int>("Product", product.Id, "ItemGroupId");
        //    var dependingPrice = _dependingPricesService.GetBestFittingDependingPriceRecord(
        //        product.Id,
        //        _services.WorkContext.CurrentCustomer,
        //        _services.WorkContext.WorkingLanguage.Id,
        //        _services.StoreContext.CurrentStore.Id,
        //        itemGroupId,
        //        quantity);
        //    decimal percentage = 0;

        //    if (dependingPrice != null)
        //    {
        //        percentage = dependingPrice.Price;
        //    }            

        //    return percentage;
        //}

		public override decimal GetFinalPrice(
			Product product, 
            Customer customer,
            decimal additionalCharge, 
            bool includeDiscounts, 
            int quantity,
			ProductBundleItemData bundleItem = null,
            PriceCalculationContext context = null,
            bool isTierPrice = false)
        {
            if (!DependingPrices.Plugin.CheckLicense())
                return base.GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem, context, isTierPrice);

            var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);
            var isVariant = product.MergedDataValues != null && product.MergedDataValues.Count > 0;
            if (isVariant)
            {
                if (!dependingPricesSettings.ProcessVariantCombinations)
                {
                    return base.GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem, context, isTierPrice);
                }
            }

			//initial price
			decimal result = product.Price;

            // Special price.
            var specialPrice = GetSpecialPrice(product);
            if (specialPrice.HasValue)
            {
                result = specialPrice.Value;
            }

            if (isTierPrice)
            {
                includeDiscounts = true;
            }

            // Tier prices.
            decimal? tierPrice = null;
            if (product.HasTierPrices && includeDiscounts && !(bundleItem != null && bundleItem.Item != null))
            {
                tierPrice = GetMinimumTierPrice(product, customer, quantity, context);
                var discountAmountTest = GetDiscountAmount(product, customer, additionalCharge, quantity, out var appliedDiscountTest, bundleItem);
                var discountProductTest = result - discountAmountTest;

                //decimal? tierPrice = GetMinimumTierPrice(product, customer, quantity);
                if (tierPrice.HasValue && tierPrice < discountProductTest)
                {
                    includeDiscounts = false;
                    result = Math.Min(result, tierPrice.Value);
                }
            }

            // BEGIN: added for plugin
			var currentCustomerRoles = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active);
			var itemGroupId = _genericAttributeService.GetAttribute<int>("Product", product.Id, "ItemGroupId");
			var dependingPrice = _dependingPricesService.GetBestFittingDependingPriceRecord(
				product.Id, 
                _services.WorkContext.CurrentCustomer, 
                _services.WorkContext.WorkingLanguage.Id, 
                _services.StoreContext.CurrentStore.Id, 
                itemGroupId,
                quantity);

			if (dependingPrice != null && dependingPrice.Price > 0)
			{
				//TODO: Option machen für Abgleich, so dass Preise nur angezeigt werden wenn der hinterlegte Preis kleiner als das Original ist
				//if (price.Price < result && price.Price != 0)
				var productPrice = Convert.ToDecimal(tierPrice != null && tierPrice > 0 && dependingPricesSettings.ApplyTierPriceAsBase ? tierPrice : product.Price);

				if (dependingPrice.Price != 0)
				{
					if (dependingPrice.CalculationMethod == DependingPriceCalculationMethod.Fixed)
					{
						result = dependingPrice.Price;
					}
					else if (dependingPrice.CalculationMethod == DependingPriceCalculationMethod.Percental)
					{
						result = productPrice - (productPrice / 100 * dependingPrice.Price);                        
                        // Calculate percental additionalCharge.
                        //additionalCharge = additionalCharge - (additionalCharge / 100 * dependingPrice.Price);
                    }
					else
					{
						result = productPrice - dependingPrice.Price;
					}
				}
			}

			// BEGIN: overwrite price if a customer role was configured to calculate with product costs
			//if (roleMatch == false && langMatch == false && storeMatch == false)
			if (dependingPrice == null)
			{ 
                if(dependingPricesSettings == null)
                {
                    return result;
                }

                foreach (var role in currentCustomerRoles)
                {
                    if (dependingPricesSettings.CustomerRolesForProductCost != null && dependingPricesSettings.CustomerRolesForProductCost.Split(',').Contains(role.Id.ToString()))
                    {
                        return product.ProductCost;
                    }

					//if (dependingPricesSettings.ApplyTierPriceAsBase)
					//{
					//	if (tierPrice != null && tierPrice > 0)
					//	{
					//		return tierPrice.Value;
					//	}
					//}
                }
            }
            // END: overwrite price if a customer role was configured to calculate with product costs

            //END: added for plugin

            // Discount + additional charge.
            if (includeDiscounts)
            {
                var discountAmount = GetDiscountAmount(product, customer, additionalCharge, quantity, out var appliedDiscount, bundleItem, context);
                result = result + additionalCharge - discountAmount;
            }
            else
            {
                result += additionalCharge;
            }

            if (result < decimal.Zero)
                result = decimal.Zero;

            return result;
        }


        /// <summary>
        /// Gets the price adjustment of a variant attribute value
        /// </summary>
        /// <param name="attributeValue">Product variant attribute value</param>
        /// <returns>Price adjustment of a variant attribute value</returns>
        public override decimal GetProductVariantAttributeValuePriceAdjustment(ProductVariantAttributeValue attributeValue,
            Product product, Customer customer, PriceCalculationContext context, int productQuantity = 1)
        {
            if (!DependingPrices.Plugin.CheckLicense() || true)
                return base.GetProductVariantAttributeValuePriceAdjustment(attributeValue, product, customer, context, productQuantity);

            Guard.NotNull(attributeValue, nameof(attributeValue));

            if (attributeValue.ValueType == ProductVariantAttributeValueType.Simple)
            {
                var itemGroupId = _genericAttributeService.GetAttribute<int>("Product", product.Id, "ItemGroupId");
                var dependingPrice = _dependingPricesService.GetBestFittingDependingPriceRecord(
                    product.Id, _services.WorkContext.CurrentCustomer, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id, itemGroupId);

                if (productQuantity > 1 && attributeValue.PriceAdjustment > 0)
                {
                    var tierPriceAttributeAdjustment = GetTierPriceAttributeAdjustment(product, customer, productQuantity, context, attributeValue.PriceAdjustment);
                    if (tierPriceAttributeAdjustment != 0)
                    {
                        if (dependingPrice != null && dependingPrice.CalculationMethod == DependingPriceCalculationMethod.Percental)
                        {
                            return tierPriceAttributeAdjustment - (tierPriceAttributeAdjustment / 100 * dependingPrice.Price);
                        }
                        else
                        {
                            return tierPriceAttributeAdjustment;
                        }
                    }
                }

                if (dependingPrice != null && dependingPrice.CalculationMethod == DependingPriceCalculationMethod.Percental)
                {
                    var priceAdjustment = attributeValue.PriceAdjustment;
                    return priceAdjustment - (priceAdjustment / 100 * dependingPrice.Price);
                }
                else
                {
                    return attributeValue.PriceAdjustment;
                }
            }
            else if (attributeValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
            {
                var linkedProduct = _productService.GetProductById(attributeValue.LinkedProductId);

                if (linkedProduct != null)
                {
                    var productPrice = GetFinalPrice(linkedProduct, true) * attributeValue.Quantity;
                    return productPrice;
                }
            }

            return decimal.Zero;
        }
    }
}
