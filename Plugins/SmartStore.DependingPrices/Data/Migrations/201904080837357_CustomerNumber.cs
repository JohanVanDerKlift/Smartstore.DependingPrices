namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DependingPrices", "CustomerNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DependingPrices", "CustomerNumber");
        }
    }
}
