namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indizes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.DependingPrices", "ProductId");
            CreateIndex("dbo.DependingPrices", "CustomerGroupId");
            CreateIndex("dbo.DependingPrices", "LanguageId");
            CreateIndex("dbo.DependingPrices", "StoreId");
            CreateIndex("dbo.DependingPrices", "ItemGroupId");
            CreateIndex("dbo.DependingPrices", "Price");
            CreateIndex("dbo.DependingPrices", "CalculationMethod");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DependingPrices", new[] { "CalculationMethod" });
            DropIndex("dbo.DependingPrices", new[] { "Price" });
            DropIndex("dbo.DependingPrices", new[] { "ItemGroupId" });
            DropIndex("dbo.DependingPrices", new[] { "StoreId" });
            DropIndex("dbo.DependingPrices", new[] { "LanguageId" });
            DropIndex("dbo.DependingPrices", new[] { "CustomerGroupId" });
            DropIndex("dbo.DependingPrices", new[] { "ProductId" });
        }
    }
}
