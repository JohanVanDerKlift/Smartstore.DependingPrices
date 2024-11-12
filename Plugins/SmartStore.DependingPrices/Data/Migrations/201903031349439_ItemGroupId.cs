namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItemGroupId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DependingPrices", "ItemGroupId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DependingPrices", "ItemGroupId");
        }
    }
}
