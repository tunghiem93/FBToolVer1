namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModTbl_Pin_ModCol_Domain : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CMS_Pin", "Domain", c => c.String(maxLength: 500));
            AlterColumn("dbo.CMS_Pin", "Repin_count", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CMS_Pin", "Repin_count", c => c.Int());
            AlterColumn("dbo.CMS_Pin", "Domain", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
