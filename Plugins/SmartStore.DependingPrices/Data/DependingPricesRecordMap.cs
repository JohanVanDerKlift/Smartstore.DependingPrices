using System.Data.Entity.ModelConfiguration;
using SmartStore.DependingPrices.Domain;

namespace SmartStore.DependingPrices.Data
{
    public partial class DependingPricesRecordMap : EntityTypeConfiguration<DependingPricesRecord>
    {
        public DependingPricesRecordMap()
        {
            this.ToTable("DependingPrices");
            this.HasKey(x => x.Id);
			this.Property(p => p.Price).HasPrecision(18, 4);
		}
    }
}