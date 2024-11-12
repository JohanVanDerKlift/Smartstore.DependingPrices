namespace SmartStore.DependingPrices.Data.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<DependingPricesObjectContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Data\Migrations";
            ContextKey = "SmartStore.DependingPrices"; // DO NOT CHANGE!
        }

        protected override void Seed(DependingPricesObjectContext context)
        {
        }
    }
}
