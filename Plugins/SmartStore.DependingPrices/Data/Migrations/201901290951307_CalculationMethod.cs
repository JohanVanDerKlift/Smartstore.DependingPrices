namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CalculationMethod : DbMigration
    {
        public override void Up()
        {
			AddColumn("dbo.DependingPrices", "CalculationMethod", c => c.Int(nullable: false));
			DropColumn("dbo.DependingPrices", "IsAdjustment");
			DropColumn("dbo.DependingPrices", "IsPercental");
		}
        
        public override void Down()
        {
			DropColumn("dbo.DependingPrices", "CalculationMethod");
		}
    }
}
