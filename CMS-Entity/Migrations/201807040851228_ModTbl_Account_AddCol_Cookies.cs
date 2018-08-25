namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModTbl_Account_AddCol_Cookies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CMS_Account", "Cookies", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CMS_Account", "Cookies");
        }
    }
}
