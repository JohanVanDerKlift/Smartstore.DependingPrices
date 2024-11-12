using System;
using SmartStore.Core;
using SmartStore.Core.Events;
using SmartStore.DependingPrices.Domain;
using SmartStore.DependingPrices.Models;
using SmartStore.DependingPrices.Services;
using SmartStore.DependingPrices.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Events;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange.Import.Events;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.DataExchange.Import;
using System.Collections.Generic;
using System.Threading;

namespace SmartStore.DependingPrices
{
	public class Events : IConsumer
	{
		private readonly ICommonServices _services;
		private readonly IDependingPricesService _dependingPricesService;
        private readonly ISettingService _settingService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IProductService _productService;

		public Events(ICommonServices services, 
			IDependingPricesService dependingPricesService, 
			ISettingService settingService,
			IGenericAttributeService genericAttributeService,
			IProductService productService)
        {
			_services = services;
			_dependingPricesService = dependingPricesService;
            _settingService = settingService;
			_genericAttributeService = genericAttributeService;
			_productService = productService;
		}
		
		public void HandleEvent(TabStripCreated eventMessage)
		{
            var tabStripName = eventMessage.TabStripName;

            if (tabStripName == "product-edit")
            {
                var entityId = ((EntityModelBase)eventMessage.Model).Id;
                var entityName = eventMessage.TabStripName.Substring(0, eventMessage.TabStripName.IndexOf("-"));

                eventMessage.ItemFactory.Add().Text(_services.Localization.GetResource("Plugins.FriendlyName.Widgets.DependingPrices"))
                    .Name("tab-dependingprices")
                    .Icon("fa fa-dollar-sign fa-lg fa-fw")
                    .LinkHtmlAttributes(new { data_tab_name = "DependingPrices" })
                    .Route("SmartStore.DependingPrices", new { action = "DependingPricesEditTab", productId = entityId })
                    .Ajax();
            }
		}

        [AdminAuthorize]
        public void HandleEvent(ModelBoundEvent eventMessage)
        {
            if (!eventMessage.BoundModel.CustomProperties.ContainsKey("DependingPrices"))
                return;

            var model = eventMessage.BoundModel.CustomProperties["DependingPrices"] as DependingPricesTabModel;
            if (model == null)
                return;

			var currentProduct = _productService.GetProductById(model.ProductId);
			_genericAttributeService.SaveAttribute(currentProduct, "ItemGroupId", model.ItemGroupId);

			if (model.Price == 0)
			{
				return;
			}
			
            var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);

            var utcNow = DateTime.UtcNow;
            var entity = _dependingPricesService.GetDependingPricesRecord(model.ProductId, model.CustomerNumber, model.CustomerGroupId, model.LanguageId, model.StoreId);
            var insert = (entity == null);
            if (entity == null)
            {
                entity = new DependingPricesRecord()
                {
                    CustomerNumber = model.CustomerNumber,
                    Quantity = model.Quantity,
                    ProductId = model.ProductId,
                    Price = model.Price,
                    CustomerGroupId = model.CustomerGroupId,
                    LanguageId = model.LanguageId,
                    StoreId = model.StoreId,
					CalculationMethod = model.CalculationMethod,
                    CreatedOnUtc = utcNow
                };
            }
            else
            {
                entity.Price = model.Price;
				entity.CalculationMethod = model.CalculationMethod;
            }

            if (insert)
            {
                _dependingPricesService.InsertDependingPricesRecord(entity);
            }
            else
            {
                _dependingPricesService.UpdateDependingPricesRecord(entity);
            }
		}

        public void HandleEvent(ImportBatchExecutedEvent<Product> eventMessage)
        {
            ProcessDependingPrices(eventMessage.Context, eventMessage.Batch);
        }

        private void ImportRecord(
            int productId,
            int itemGroupId,
            int customerGroupId, 
            int languageId, 
            int storeId,
            int quantity,
            string customerNumber, 
            decimal price, 
            string calculationMethod, 
            bool deleteRecord)
        {

            var dependingPricesRecord = _dependingPricesService.GetDependingPricesRecord(productId, customerNumber, customerGroupId, languageId, storeId, itemGroupId);

            var isNew = false;

            if (dependingPricesRecord == null)
            {
                dependingPricesRecord = new DependingPricesRecord();
                isNew = true;
            }
            else if (deleteRecord == true)
            {
                _dependingPricesService.DeleteDependingPricesRecord(dependingPricesRecord);
                return;
            }

            dependingPricesRecord.ProductId = productId;
            dependingPricesRecord.CustomerGroupId = customerGroupId;
            dependingPricesRecord.LanguageId = languageId;
            dependingPricesRecord.StoreId = storeId;
            dependingPricesRecord.ItemGroupId = itemGroupId;
            dependingPricesRecord.Quantity = itemGroupId;
            dependingPricesRecord.CustomerNumber = customerNumber;
            dependingPricesRecord.Price = price;

            switch (calculationMethod)
            {
                case "Fixed":
                    dependingPricesRecord.CalculationMethod = DependingPriceCalculationMethod.Fixed;
                    break;
                case "Percental":
                    dependingPricesRecord.CalculationMethod = DependingPriceCalculationMethod.Percental;
                    break;
                case "Adjustment":
                    dependingPricesRecord.CalculationMethod = DependingPriceCalculationMethod.Adjustment;
                    break;
            }

            if (isNew)
            {
                _dependingPricesService.InsertDependingPricesRecord(dependingPricesRecord);
            }
            else
            {
                _dependingPricesService.UpdateDependingPricesRecord(dependingPricesRecord);
            }
        }

        private int ProcessDependingPrices(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
        {
            foreach (var row in batch)
            {
                try
                {
                    //var product = row.Entity;
                    //var productId = row.GetDataValue<string>("ID").Convert<int>();
                    var productId = row.Entity.Id;
                    var itemGroupId = row.GetDataValue<string>("DP_ItemGroupId").Convert<int>();

                    // 1:99,9;2:89,9; 1&2 
                    var singleLineGroupPrices = row.GetDataValue<string>("DP_CustomerGroupPrices").Convert<string>();
                    
                    // if there is no DP_ProductId or DP_ItemGroupId get outta hee
                    if (productId == 0 && itemGroupId == 0)
                        continue;

                    var customerGroupId = row.GetDataValue<string>("DP_CustomerGroupId").Convert<int>();
                    var languageId = row.GetDataValue<string>("DP_LanguageId").Convert<int>();
                    var storeId = row.GetDataValue<string>("DP_StoreId").Convert<int>();
                    var customerNumber = row.GetDataValue<string>("DP_CustomerNumber");
                    var price = row.GetDataValue<string>("DP_Price").Convert<decimal>();
                    var calculationMethod = row.GetDataValue<string>("DP_CalculationMethod");
                    var quantity = row.GetDataValue<int>("DP_Quantity");
                    var deleteRecord = row.GetDataValue<bool>("DP_Delete");

                    if (!singleLineGroupPrices.HasValue())
                    {
                        ImportRecord(productId, itemGroupId, customerGroupId, languageId, storeId, quantity, customerNumber, price, calculationMethod, deleteRecord);
                    }
                    else
                    {
                        var lines = singleLineGroupPrices.Split(';');
                        foreach(var line in lines)
                        {
                            var priceData = line.Split(':');
                            var lineCustomerGroupId = Convert.ToInt32(priceData[0]);
                            var linePrice = Convert.ToDecimal(priceData[1]);
                            ImportRecord(productId, itemGroupId, lineCustomerGroupId, languageId, storeId, quantity, customerNumber, linePrice, calculationMethod, deleteRecord);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // Context can be null so this event can be used by other plugins to fill the database
                    if (context != null)
                    {
                        context.Result.AddWarning(ex.Message, row.GetRowInfo(), "DependingPrices");
                    }
                }
            }

            return 0;
        }
    }
}