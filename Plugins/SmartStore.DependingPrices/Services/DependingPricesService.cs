using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.DependingPrices.Domain;
using SmartStore.Services.Catalog;

namespace SmartStore.DependingPrices.Services
{
    public partial class DependingPricesService : IDependingPricesService
    {
        private readonly IRepository<DependingPricesRecord> _cbRepository;
		private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
		private readonly IDbContext _dbContext;
		private readonly AdminAreaSettings _adminAreaSettings;

        public DependingPricesService(
            IRepository<DependingPricesRecord> cbRepository,
			IProductService productService,
			IDbContext dbContext,
			AdminAreaSettings adminAreaSettings,
            IComponentContext ctx, ICategoryService categoryService)
        {
            _cbRepository = cbRepository;
			_productService = productService;
			_dbContext = dbContext;
			_adminAreaSettings = adminAreaSettings;
            _categoryService = categoryService;
        }

		public IPagedList<DependingPricesRecord> GetAllDependingPricesRecords(
            int productId, 
            int itemGroupId = 0, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue, 
            bool showHidden = false)
		{
			var dependingPrices = GetDependingPricesRecords(productId, itemGroupId);

			return new PagedList<DependingPricesRecord>(dependingPrices, pageIndex, pageSize);
		}

		public List<DependingPricesRecord> GetDependingPricesRecords(int productId, int itemGroupId = 0)
        {
            //if (productId == 0)
            //    return null; 

			if (itemGroupId != 0)
			{
				var query =
					from gp in _cbRepository.Table
					where gp.ItemGroupId == itemGroupId
					select gp;

				return query.ToList();
			}
			else if (productId != 0 && productId != -1)
			{
				var query =
					from gp in _cbRepository.Table
					where gp.ProductId == productId
					select gp;

				return query.ToList();
			}
			else
			{
				var query =
					from gp in _cbRepository.Table
					where gp.ItemGroupId != 0 && gp.ItemGroupId != null
					select gp;

				return query.ToList();
			}
        }

		public DependingPricesRecord GetBestFittingDependingPriceRecord(
			int productId, 
			Customer customer, 
			int langId, 
			int storeId, 
			int itemGroupId = 0, 
			int quantity = 1)

		{
			// Fix quantity input for lists 
			if (quantity > 20000000)
				quantity = 1;

			var dependingPrices = new List<DependingPricesRecord>();
            var customerRoles = customer.CustomerRoleMappings.Select(x => x.CustomerRole).Where(cr => cr.Active);
			var dependingPrice = new DependingPricesRecord();
            var tempDependingPrice = new DependingPricesRecord();
            var roles = (from gp in _cbRepository.Table select gp.CustomerGroupId).Distinct().ToList();

            // KOOMBA
            // Get discount percentage based on input parameters. Return the highest percentage.

            // 1 - On customerNumber, productId and quantity
            if (customer.CustomerNumber.HasValue() && productId != 0)
            { 
                var result = (from gp in _cbRepository.Table                                 
                              where gp.Quantity <= quantity
                              && gp.CustomerNumber == customer.CustomerNumber
                              && gp.ProductId == productId
                              orderby gp.Quantity descending
                              select gp).ToList().FirstOrDefault();

                if (result != null)
                {
                    dependingPrice = result;
                }                                                
            }

            //2 - On productId, customerRole and quantity
            if (productId != 0 && customerRoles != null)
            {                               
                foreach (var role in customerRoles)
                    if (roles.Contains(role.Id))
                    {
                        tempDependingPrice = (from gp in _cbRepository.Table                                          
                                              where gp.CustomerGroupId == role.Id
                                              && gp.ProductId == productId
                                              && gp.Quantity <= quantity
                                              orderby gp.Quantity descending
                                              select gp).ToList().FirstOrDefault();

                        if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
                        {                            
                            dependingPrice = tempDependingPrice;                                                    
                        }
                    }

                //var dependingPriceOnProduct = (from gp in _cbRepository.Table
                //                               orderby gp.Quantity descending
                //                               where gp.Quantity <= quantity
                //                               && gp.ProductId == productId
                //                               select gp).ToList().FirstOrDefault();

                //if (dependingPriceOnProduct != null)
                //{
                //    if (dependingPrice == null || dependingPriceOnProduct.Price > dependingPrice.Price)
                //    {
                //        dependingPrice = dependingPriceOnProduct;
                //    }
                //}
            }

            // 3 - On CustomerNumber, itemGroupId and quantity
            if (customer.CustomerNumber.HasValue() && itemGroupId != 0)
            {
                tempDependingPrice = (from gp in _cbRepository.Table                                      
                                      where gp.CustomerNumber == customer.CustomerNumber
                                      && gp.ItemGroupId == itemGroupId
                                      && gp.Quantity <= quantity
                                      orderby gp.Quantity descending
                                      select gp).ToList().FirstOrDefault();

                if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
                {
                    dependingPrice = tempDependingPrice;
                }
            }

            // 4 - On CustomerRole, itemGroupId and quantity
            if (itemGroupId != 0 && customerRoles != null)
            {
                foreach (var role in customerRoles)
                    if (roles.Contains(role.Id))
                    {
                        tempDependingPrice = (from gp in _cbRepository.Table                                              
                                              where gp.CustomerGroupId == role.Id
                                              && gp.ItemGroupId == itemGroupId
                                              && gp.Quantity <= quantity
                                              orderby gp.Quantity descending
                                              select gp).ToList().FirstOrDefault();

                        if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
                        {
                            dependingPrice = tempDependingPrice;
                        }

                    }

                //var tempDependingPrices = new List<DependingPricesRecord>();
                //tempDependingPrices = GetDependingPricesRecords(0, itemGroupId)
                //    .OrderByDescending(x => x.Quantity).ToList();
                //foreach (var role in customerRoles)
                //{
                //    tempDependingPrice = tempDependingPrices
                //        .Where(x => x.CustomerGroupId == role.Id && x.Quantity <= quantity)
                //        .FirstOrDefault();
                //    if (tempDependingPrice != null)
                //    {
                //        if (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price)
                //        {
                //            dependingPrice = tempDependingPrice;
                //        }
                //    }
                //}              
            }

            // 5 - On productId and quantity
            if (productId != 0)
            {
                tempDependingPrice = (from gp in _cbRepository.Table                                     
                                      where gp.Quantity <= quantity
                                      && gp.ProductId == productId
                                      orderby gp.Quantity descending
                                      select gp).ToList().FirstOrDefault();

                if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
                {
                    dependingPrice = tempDependingPrice;
                }
            }

            // 6 - On itemGroupId and quantity
            if (itemGroupId != 0)
            {
                tempDependingPrice = (from gp in _cbRepository.Table                                      
                                      where gp.CustomerNumber == "0"
                                      && gp.CustomerGroupId == 0
                                      && gp.ProductId == 0
                                      && gp.ItemGroupId == itemGroupId
                                      && gp.Quantity <= quantity
                                      orderby gp.Quantity descending
                                      select gp).ToList().FirstOrDefault();

                if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
                {
                    dependingPrice = tempDependingPrice;
                }
            }

            // 7 - If no parameters are specified, special price
            tempDependingPrice = (from gp in _cbRepository.Table
                                  where gp.CustomerNumber == "0"
                                  && gp.CustomerGroupId == 0
                                  && gp.ProductId == 0
                                  && gp.ItemGroupId == 0
                                  && gp.Quantity <= quantity
                                  orderby gp.Quantity
                                  select gp).ToList().FirstOrDefault();

            if (tempDependingPrice != null && (dependingPrice == null || tempDependingPrice.Price > dependingPrice.Price))
            {
                dependingPrice = tempDependingPrice;
            }

            return dependingPrice;

            ////////////////
            //    dependingPrices = GetDependingPricesRecords(productId);

            //    // Return customer record directly as it is explicilty defined for a customer with no regards of store, language or whatever
            //    if (dependingPrices.Count != 0 && customer.CustomerNumber.HasValue())
            //    {
            //        var customerRecord = dependingPrices.Where(x => x.CustomerNumber == customer.CustomerNumber).FirstOrDefault();
            //        if (customerRecord != null)
            //        {
            //            if (quantity >= 1)
            //            {
            //                customerRecord = dependingPrices
            //                    .Where(x => (x.CustomerNumber == customer.CustomerNumber) && x.Quantity <= quantity && x.Quantity != 0 && x.Quantity != null)
            //                    .OrderByDescending(x => x.Quantity)
            //                    .FirstOrDefault();
            //            }
            //            else
            //            {
            //                customerRecord = dependingPrices
            //                    .Where(x => (x.CustomerNumber == customer.CustomerNumber) && x.Quantity == quantity)
            //                    .FirstOrDefault();
            //            }

            //            if (customerRecord != null)
            //                return customerRecord;
            //        }
            //        else
            //        {
            //            dependingPrices = dependingPrices.Where(x => x.CustomerNumber == string.Empty || x.CustomerNumber == null || x.CustomerNumber.Equals('0')).ToList();
            //        }
            //        //dependingPrices = dependingPrices.Where(x => x.CustomerNumber == customer.CustomerNumber || (x.CustomerNumber != null && x.CustomerNumber.Equals("*"))).ToList();
            //    }
            //    else
            //    {
            //        dependingPrices = dependingPrices.Where(x => x.CustomerNumber == string.Empty || x.CustomerNumber == null).ToList();
            //    }

            //    if (itemGroupId != 0 && dependingPrices.Count == 0)
            //    {
            //        dependingPrices = GetDependingPricesRecords(0, itemGroupId);
            //    }

            //    if (itemGroupId != 0)
            //    {
            //        dependingPrices.AddRange(GetDependingPricesRecords(0, itemGroupId));

            //        var customerRecord = dependingPrices
            //            .Where(x => x.CustomerNumber == customer.CustomerNumber && x.Quantity <= quantity)
            //            .OrderBy(x => x.Quantity)
            //            //.OrderBy(x => x.Price)
            //            .FirstOrDefault();

            //        if (customerRecord != null)
            //        {
            //            if (quantity >= 1)
            //            {
            //                customerRecord = dependingPrices
            //                    .Where(x => (x.CustomerNumber == customer.CustomerNumber) && x.Quantity <= quantity && x.Quantity != 0 && x.Quantity != null)
            //                    .OrderByDescending(x => x.Quantity)
            //                    .FirstOrDefault();
            //            }
            //            else
            //            {
            //                customerRecord = dependingPrices
            //                    .Where(x => (x.CustomerNumber == customer.CustomerNumber) && x.Quantity == quantity)
            //                    .FirstOrDefault();
            //            }

            //            return customerRecord;
            //        }
            //    }

            //    if (langId != 0)
            //        dependingPrices = dependingPrices.Where(x => x.LanguageId == langId || x.LanguageId == 0).ToList();

            //    if (storeId != 0)
            //        dependingPrices = dependingPrices.Where(x => x.StoreId == storeId || x.StoreId == 0).ToList();

            //    if (quantity >= 1)
            //    {
            //        dependingPrices = dependingPrices
            //            .Where(x => x.Quantity <= quantity) // && x.Quantity != 0 && x.Quantity != null)
            //            .OrderByDescending(x => x.Quantity)
            //            //.OrderBy(x => x.Price)
            //            .ToList();
            //    }
            //    else
            //    {
            //        dependingPrices = dependingPrices
            //            .Where(x => x.Quantity == quantity)
            //            .ToList();
            //    }

            //    var tempDP = new DependingPricesRecord();
            //    //var allCustomerRolesRecord = dependingPrices.Where(x => x.CustomerGroupId == 0).FirstOrDefault();
            //    //if (allCustomerRolesRecord != null)
            //    //{
            //    //	tempDP = allCustomerRolesRecord;
            //    //}

            //    foreach (var role in customerRoles)
            //    {
            //        // get cheapest price for current customer role
            //        var newTempDP = dependingPrices.Where(x => x.CustomerGroupId == role.Id).FirstOrDefault();
            //        if (newTempDP == null)
            //        {
            //            // Only get new record if we don't have already a customer role specific price
            //            if (tempDP.CustomerGroupId == 0 || tempDP.CustomerGroupId == null)
            //            {
            //                newTempDP = dependingPrices.Where(x => x.CustomerGroupId == 0).FirstOrDefault();
            //            }
            //        }

            //        // TODO: this aint the cheapest price, respect calculation method
            //        //if (newTempDP != null && (tempDP == null || tempDP.Price == 0 || tempDP.Price < newTempDP.Price))
            //        if (newTempDP != null && (tempDP == null || tempDP.Price == 0 || newTempDP.Price > tempDP.Price))
            //        {
            //            tempDP = newTempDP;
            //            //tempDP = dependingPrices.Where(x => x.CustomerGroupId == 9).FirstOrDefault();
            //        }
            //    }

            //    // INFO: Added as requested on 12.02.2024
            //    // Check if there is a record defined to work for all customer roles.


            //    return tempDP.Id > 0 ? tempDP : null;
        }

        public DependingPricesRecord GetDependingPricesRecord(int productId, string customerNumber, int customerGroupId, int langId, int storeId, int itemGroupId = 0, int quantity = 1)
        {
            if (productId == 0 && itemGroupId == 0)
                return null;

            var record = new DependingPricesRecord();

            if (customerNumber.HasValue())
            {
                var query =
                    from gp in _cbRepository.Table
                    where gp.CustomerNumber.Equals(customerNumber) && gp.ProductId == productId
                    select gp;

                record = query.FirstOrDefault();
            }
			else if (itemGroupId != 0)
			{
				var query =
					from gp in _cbRepository.Table
					where gp.ItemGroupId == itemGroupId && gp.CustomerGroupId == customerGroupId && gp.LanguageId == langId && gp.StoreId == storeId
					select gp;

				record = query.FirstOrDefault();
			}
			else
			{
				var query =
					from gp in _cbRepository.Table
					where gp.ProductId == productId && gp.CustomerGroupId == customerGroupId && gp.LanguageId == langId && gp.StoreId == storeId && gp.Quantity == quantity
					select gp;

				record = query.FirstOrDefault();
			}
            
            return record;
        }

        public DependingPricesRecord GetDependingPricesRecordById(int dpId)
        {
            if (dpId == 0)
                return null;

            var record = new DependingPricesRecord();

            var query =
                from gp in _cbRepository.Table
                where gp.Id == dpId
                select gp;

            record = query.FirstOrDefault();

            return record;
        }

        public void InsertDependingPricesRecord(DependingPricesRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("DependingPricesRecord");

            record.CreatedOnUtc = DateTime.UtcNow;

            _cbRepository.Insert(record);
        }

        public void UpdateDependingPricesRecord(DependingPricesRecord record)
        {
			if (record == null)
				throw new ArgumentNullException("record");

            record.UpdatedOnUtc = DateTime.UtcNow;

            _cbRepository.Update(record);
        }

        public void DeleteDependingPricesRecord(DependingPricesRecord record)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			_cbRepository.Delete(record);
		}

		
	}
}
