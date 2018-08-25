namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModTbl_Pin_addCol_DaysCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CMS_Pin", "DayCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CMS_Pin", "DayCount");
        }
    }
}
