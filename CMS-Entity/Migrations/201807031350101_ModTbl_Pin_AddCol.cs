namespace CMS_Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModTbl_Pin_AddCol : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CMS_Pin", "ReactionCount", c => c.Int(nullable: false));
            AddColumn("dbo.CMS_Pin", "ShareCount", c => c.Int(nullable: false));
            AddColumn("dbo.CMS_Pin", "CommentCount", c => c.Int(nullable: false));
            AddColumn("dbo.CMS_Pin", "Description", c => c.String(maxLength: 2000));
            AddColumn("dbo.CMS_Pin", "OwnerId", c => c.String());
            AddColumn("dbo.CMS_Pin", "OwnerName", c => c.String());
            AlterColumn("dbo.CMS_Pin", "Created_At", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CMS_Pin", "Created_At", c => c.DateTime());
            DropColumn("dbo.CMS_Pin", "OwnerName");
            DropColumn("dbo.CMS_Pin", "OwnerId");
            DropColumn("dbo.CMS_Pin", "Description");
            DropColumn("dbo.CMS_Pin", "CommentCount");
            DropColumn("dbo.CMS_Pin", "ShareCount");
            DropColumn("dbo.CMS_Pin", "ReactionCount");
        }
    }
}
