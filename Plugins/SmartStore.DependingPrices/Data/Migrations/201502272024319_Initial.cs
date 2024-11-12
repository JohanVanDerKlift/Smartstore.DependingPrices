namespace SmartStore.DependingPrices.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DependingPrices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CustomerGroupId = c.Int(nullable: true),
                        LanguageId = c.Int(nullable: true),
                        StoreId = c.Int(nullable: true),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 4),
                        IsAdjustment = c.Boolean(nullable: false),
                        IsPercental = c.Boolean(nullable: false),
                        CreatedOnUtc = c.DateTime(),
                        UpdatedOnUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.DependingPrices");
        }
    }
}
