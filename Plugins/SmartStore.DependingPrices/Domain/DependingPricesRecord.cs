using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core;

namespace SmartStore.DependingPrices.Domain
{
    /// <summary>
    /// Represents a depending prices record
    /// </summary>
    public partial class DependingPricesRecord : BaseEntity
    {
		[Index("IX_ProductId", 0)]
		public int ProductId { get; set; }

		[Index("IX_CustomerGroupId", 1)]
		public int? CustomerGroupId { get; set; }

		[Index("IX_LanguageId", 2)]
		public int? LanguageId { get; set; }

		[Index("IX_StoreId", 3)]
		public int? StoreId { get; set; }

		[Index("IX_ItemGroupId", 4)]
		public int? ItemGroupId { get; set; }
		public int? Quantity { get; set; }

		public string CustomerNumber { get; set; }

		[Index("IX_Price", 5)]
		public decimal Price { get; set; }

		[Index("IX_CalculationMethod", 6)]
		public DependingPriceCalculationMethod CalculationMethod { get; set; }

		public DateTime? CreatedOnUtc { get; set; }
		public DateTime? UpdatedOnUtc { get; set; }
    }

	public enum DependingPriceCalculationMethod
	{
		Fixed = 0,
		Percental = 5,
		Adjustment = 10
	}
}