using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DependingPrices.Models
{
    public class PublicInfoModel : ModelBase
    {
		public bool HideTierPrices { get; set; } = false;
		public bool HidePrice { get; set; } = false;
		public int ProductId { get; set; }
	}
}