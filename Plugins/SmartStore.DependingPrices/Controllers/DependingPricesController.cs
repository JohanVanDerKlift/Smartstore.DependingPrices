using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.DependingPrices.Domain;
using SmartStore.DependingPrices.Models;
using SmartStore.DependingPrices.Services;
using SmartStore.DependingPrices.Settings;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Models.Catalog;
using Telerik.Web.Mvc;

namespace SmartStore.DependingPrices.Controllers
{
    public class DependingPricesController : AdminControllerBase
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IDependingPricesService _dependingPricesService;
        private readonly ICustomerService _customerService;
        private readonly ICommonServices _services;
        private readonly ILanguageService _languageService;
		private readonly IGenericAttributeService _genericAttributeService;

		public DependingPricesController(
			ISettingService settingService,
			IStoreService storeService,
            IDependingPricesService dependingPricesService, 
            ICustomerService customerService, 
            ICommonServices services, 
			ILanguageService languageService,
			IGenericAttributeService genericAttributeService)
        {
            _settingService = settingService;
            _storeService = storeService;
            _dependingPricesService = dependingPricesService;
            _customerService = customerService;
            _services = services;
            _languageService = languageService;
			_genericAttributeService = genericAttributeService;
		}
        
        [AdminAuthorize]
        [ChildActionOnly]
		[LoadSetting]
		public ActionResult Configure(DependingPricesSettings settings)
        {
            var model = new ConfigurationModel();
			MiniMapper.Map(settings, model);

			model.CustomerRolesForPriceSupression = settings.CustomerRolesForPriceSupression.SplitSafe(",");
			model.CustomerRolesForProductCost = settings.CustomerRolesForProductCost.SplitSafe(",");

			PrepareModel(model, settings);

			return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
		[SaveSetting]
        public ActionResult Configure(ConfigurationModel model, DependingPricesSettings settings, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure(settings);
            
			MiniMapper.Map(model, settings);

			return RedirectToConfiguration("SmartStore.DependingPrices");
		}

		private void PrepareModel(ConfigurationModel model, DependingPricesSettings settings)
        {
			var list1 = new List<SelectListItem>
			{
				new SelectListItem { Text = _services.Localization.GetResource("Admin.Common.All"), Value = "0" }
			};

			var list2 = new List<SelectListItem>
			{
				new SelectListItem { Text = _services.Localization.GetResource("Admin.Common.All"), Value = "0" }
			};

			foreach (var cr in _customerService.GetAllCustomerRoles(true))
			{
				list1.Add(new SelectListItem()
				{
					Text = cr.Name,
					Value = cr.Id.ToString(),
					Selected = settings.CustomerRolesForProductCost != null && settings.CustomerRolesForProductCost.Contains(cr.Id.ToString())
				});

				list2.Add(new SelectListItem()
				{
					Text = cr.Name,
					Value = cr.Id.ToString(),
					Selected = settings.CustomerRolesForPriceSupression != null && settings.CustomerRolesForPriceSupression.Contains(cr.Id.ToString())
				});
			}

			model.AvailableCustomerRoles = new MultiSelectList(list1, "Value", "Text", model.CustomerRolesForPriceSupression);
			model.AvailableCustomerRoles2 = new MultiSelectList(list2, "Value", "Text", model.CustomerRolesForPriceSupression);
		}

		public ActionResult PublicInfo(string widgetZone, object model)
		{
			var publicInfoModel = new PublicInfoModel();
			var routeData = this.ControllerContext.GetRootControllerContext().RouteData;
			var routeId = routeData.GenerateRouteIdentifier().ToLower();
			var productId = routeData.Values["productid"];

			if (productId != null)
			{
				var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);
				var currentCustomerRoles = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active);
				var itemGroupId = _genericAttributeService.GetAttribute<int>("Product", (int)productId, "ItemGroupId");
				var dependingPrice = _dependingPricesService.GetBestFittingDependingPriceRecord(
					(int)productId, _services.WorkContext.CurrentCustomer, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id, itemGroupId);

				if (dependingPrice != null && dependingPrice.Price > 0)
				{
					publicInfoModel.HideTierPrices = dependingPricesSettings.HideTierPrices;	
				}
				else
				{
					foreach (var role in currentCustomerRoles)
					{
						if (dependingPricesSettings.CustomerRolesForProductCost.Split(',').Contains(role.Id.ToString()))
						{
							publicInfoModel.HideTierPrices = dependingPricesSettings.HideTierPrices;
							break;
						}
					}

					foreach (var role in currentCustomerRoles)
					{
						if (dependingPricesSettings.CustomerRolesForPriceSupression.HasValue() && dependingPricesSettings.CustomerRolesForPriceSupression.Split(',').Contains(role.Id.ToString()))
						{
							publicInfoModel.HidePrice = true;
							break;
						}
					}
				}
			}

			return View(publicInfoModel);
		}

		public ActionResult ListItemPublicInfo(string widgetZone, object model)
		{
			var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);
			if (!dependingPricesSettings.CustomerRolesForPriceSupression.HasValue())
				return new EmptyResult();

			var publicInfoModel = new PublicInfoModel();
			if (model != null && model.GetType() == typeof(ProductSummaryModel.SummaryItem))
			{
				var overViewModel = (ProductSummaryModel.SummaryItem)model;
				int productId = overViewModel.Id;

				if (productId != 0)
				{
					var currentCustomerRoles = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active);
					var itemGroupId = _genericAttributeService.GetAttribute<int>("Product", productId, "ItemGroupId");
					var dependingPrice = _dependingPricesService.GetBestFittingDependingPriceRecord(
						productId, _services.WorkContext.CurrentCustomer, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id, itemGroupId);

					if (dependingPrice == null || dependingPrice.Price == 0)
					{
						foreach (var role in currentCustomerRoles)
						{
							if (dependingPricesSettings.CustomerRolesForPriceSupression.Split(',').Contains(role.Id.ToString()))
							{
								publicInfoModel.ProductId = productId;
								publicInfoModel.HidePrice = true;
								break;
							}
						}
					}
				}
			}

			return View(publicInfoModel);
		}

		[AdminAuthorize]
        public ActionResult DependingPricesEditTab(int productId)
        {
   //         var dependingPricesSettings = _settingService.LoadSetting<DependingPricesSettings>(_services.StoreContext.CurrentStore.Id);

   //         var dependingPricesRecords = new List<DependingPricesRecord>();

			//dependingPricesRecords = _dependingPricesService.GetDependingPricesRecords(productId);

            var model = new DependingPricesTabModel();
			model.ProductId = productId;

			if (productId == -1)
			{
				model.IsPluginConfig = true;
			}
			
			model.AvailableCustomerRoles.Add(new SelectListItem { Text = _services.Localization.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var cr in _customerService.GetAllCustomerRoles(true))
            {
                model.AvailableCustomerRoles.Add(new SelectListItem() { Text = cr.Name, Value = cr.Id.ToString(), Selected = false });
            }

            model.AvailableStores.Add(new SelectListItem { Text = _services.Localization.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var store in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            }

            model.AvailableLanguages.Add(new SelectListItem() { Text = _services.Localization.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var language in _languageService.GetAllLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem() { Text = language.Name, Value = language.Id.ToString() });
            }

			model.ItemGroupId = _genericAttributeService.GetAttribute<int>("Product", productId, "ItemGroupId");

			var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[DependingPrices]" };
            return result;
        }

		// For record saving in plugin config
		[HttpPost]
		public ActionResult AddCriteria(
			int itemGroupId, 
			decimal price,
			int quantity,
			string calculationMethod, 
			int customerGroupId, 
			int storeId, 
			int languageId, 
			string customerNumber)
		{
			var utcNow = DateTime.UtcNow;
			var entity = _dependingPricesService.GetDependingPricesRecord(0, customerNumber, customerGroupId, languageId, storeId, itemGroupId, quantity);
			var insert = (entity == null);
			if (entity == null)
			{
				entity = new DependingPricesRecord()
				{
					ItemGroupId = itemGroupId,
                    CustomerNumber = customerNumber,
					Quantity = quantity,
					ProductId = 0,
					Price = price,
					CustomerGroupId = customerGroupId,
					LanguageId = languageId,
					StoreId = storeId,
					CalculationMethod = (DependingPriceCalculationMethod)Enum.Parse(typeof(DependingPriceCalculationMethod), calculationMethod),
					CreatedOnUtc = utcNow
				};
			}
			else
			{
				entity.Price = price;
				entity.Quantity = quantity;
				entity.CalculationMethod = (DependingPriceCalculationMethod)Enum.Parse(typeof(DependingPriceCalculationMethod), calculationMethod);
			}

			entity.UpdatedOnUtc = utcNow;

			if (insert)
			{
				_dependingPricesService.InsertDependingPricesRecord(entity);
			}
			else
			{
				_dependingPricesService.UpdateDependingPricesRecord(entity);
			}

			return new JsonResult { Data = true };
		}

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CriteriaList(GridCommand command, int productId)
        {
            var dependingPricesRecords = _dependingPricesService.GetAllDependingPricesRecords(productId, 0, command.Page - 1, command.PageSize, true);
            var filters = new List<FilterCriteria>();

            foreach (var price in dependingPricesRecords)
            {
                var langName = price.LanguageId == 0 ? T("Admin.Common.All").Text : _languageService.GetLanguageById((int)price.LanguageId).Name;
                var customerRoleName = price.CustomerGroupId == 0 ? T("Admin.Common.All").Text :_customerService.GetCustomerRoleById((int)price.CustomerGroupId).Name;
                var storeName = price.StoreId == 0 ? T("Admin.Common.All").Text :_storeService.GetStoreById((int)price.StoreId).Name;
				var calcName = T("Admin.Product.Price.Tierprices.Fixed").Text;

				if (price.CalculationMethod == DependingPriceCalculationMethod.Adjustment)
					calcName = T("Admin.Product.Price.Tierprices.Adjustment").Text;

				if (price.CalculationMethod == DependingPriceCalculationMethod.Percental)
					calcName = T("Admin.Product.Price.Tierprices.Percental").Text;

				filters.Add(new FilterCriteria()
                { 
                    Id = price.Id,
					FilterDecimal = price.Price,
                    Language = langName,
					Quantity = price.Quantity,
					LanguageId = price.LanguageId,
					CustomerGroup = customerRoleName,
					CustomerGroupId = price.CustomerGroupId,
                    CustomerNumber = price.CustomerNumber,
					CalculationMethodId = (int)price.CalculationMethod,
					CalculationMethod = calcName,
					Store = storeName,
					StoreId = price.StoreId,
					ProductId = price.ProductId,
					ItemGroupId = Convert.ToInt32(price.ItemGroupId)
				});
            }

            var model = new GridModel<FilterCriteria>
            {
                Data = filters,
                Total = dependingPricesRecords.TotalCount
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DependingPriceUpdate(GridCommand command, FilterCriteria model)
        {
            var dp = _dependingPricesService.GetDependingPricesRecordById(model.Id);
			//var price = Request.Form["CustomProperties[DependingPrices].FilterDecimal"];
			//var itemGroupId = Request.Form["CustomProperties[DependingPrices].ItemGroupId"];
			//var customerNumber = Request.Form["CustomProperties[DependingPrices].CustomerNumber"];

			var price = Request.Form["FilterDecimal"];
			var itemGroupId = Request.Form["ItemGroupId"];
			var customerNumber = Request.Form["CustomerNumber"];
			var quantity = Request.Form["Quantity"];

			dp.Price = decimal.Parse(price);
			dp.CalculationMethod = (DependingPriceCalculationMethod)model.CalculationMethod.ToInt();
			dp.CustomerGroupId = model.CustomerGroup.IsNumeric() && Int32.Parse(model.CustomerGroup) != 0 ? Int32.Parse(model.CustomerGroup) : 0;
			dp.LanguageId = model.Language.ToInt();
			dp.StoreId = model.Store.ToInt();
            dp.CustomerNumber = customerNumber;
			dp.Quantity = !quantity.HasValue() ? 0 : Int32.Parse(quantity);
			dp.ItemGroupId = itemGroupId == null ? 0 : Int32.Parse(itemGroupId);

			_dependingPricesService.UpdateDependingPricesRecord(dp);

			return CriteriaList(command, dp.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult DependingPriceDelete(int id, GridCommand command)
        {
            var dp = _dependingPricesService.GetDependingPricesRecordById(id);
            if (dp == null)
                throw new ArgumentException("No depending price found with the specified id");

            _dependingPricesService.DeleteDependingPricesRecord(dp);

            return CriteriaList(command, dp.ProductId);
        }

		public ActionResult AllLanguages(string label, int selectedId = 0)
		{
			var languages = _languageService.GetAllLanguages(true)
				.Select(x => new 
				{
					selected = (x.Id.Equals(selectedId)),
					text = x.Name,
					id = x.Id.ToString()
				}).ToList();

			return new JsonResult { Data = languages, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}
	}
}