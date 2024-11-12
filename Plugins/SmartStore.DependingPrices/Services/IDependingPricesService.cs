using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.DependingPrices.Domain;

namespace SmartStore.DependingPrices.Services
{
    public partial interface IDependingPricesService
    {
		IPagedList<DependingPricesRecord> GetAllDependingPricesRecords(int productId, int itemGroupId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
		List<DependingPricesRecord> GetDependingPricesRecords(int productId, int itemGroupId = 0);
        DependingPricesRecord GetDependingPricesRecord(int productId, string customerNumber, int customerGroupId, int langId, int storeId, int itemGroupId = 0, int quantity = 1);
        DependingPricesRecord GetDependingPricesRecordById(int dpId);
		DependingPricesRecord GetBestFittingDependingPriceRecord(int productId, Customer customer, int langId, int storeId, int itemGroupId = 0, int quantity = 1);

        void InsertDependingPricesRecord(DependingPricesRecord record);
        void UpdateDependingPricesRecord(DependingPricesRecord record);
        void DeleteDependingPricesRecord(DependingPricesRecord record);
    }
}
