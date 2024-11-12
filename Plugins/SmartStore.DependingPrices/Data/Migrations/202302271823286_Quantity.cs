namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DependingPrices", "Quantity", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DependingPrices", "Quantity");
        }
    }
}
